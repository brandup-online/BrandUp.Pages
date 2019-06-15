using BrandUp.Pages.Administration;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class ContentEditorRepository : IContentEditorStore
    {
        private readonly IMongoCollection<ContentEditorDocument> documents;

        public ContentEditorRepository(IPagesDbContext dbContext)
        {
            documents = dbContext.ContentEditors;
        }

        public IQueryable<IContentEditor> ContentEditors => documents.AsQueryable();

        public async Task AssignEditorAsync(string email, CancellationToken cancellationToken = default)
        {
            await documents.InsertOneAsync(new ContentEditorDocument
            {
                Id = ObjectId.GenerateNewId(),
                CreatedDate = DateTime.UtcNow,
                Version = 1,
                Email = email
            }, new InsertOneOptions(), cancellationToken);
        }

        public async Task<IContentEditor> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<ContentEditorDocument>.Filter.Eq(it => it.Id, new ObjectId(id));
            var cursor = await documents.FindAsync(filter);
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<IContentEditor> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var filter = Builders<ContentEditorDocument>.Filter.Eq(it => it.Email, email);
            var cursor = await documents.FindAsync(filter);
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task DeleteAsync(IContentEditor contentEditor, CancellationToken cancellationToken = default)
        {
            var contentEditorDocument = (ContentEditorDocument)contentEditor;
            var curVersion = contentEditorDocument.Version;
            contentEditorDocument.Version++;

            var deleteResult = await documents.DeleteOneAsync(it => it.Id == contentEditorDocument.Id && it.Version == curVersion, cancellationToken: cancellationToken);
            if (deleteResult.DeletedCount != 1)
                throw new InvalidOperationException();
        }
    }
}