using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageEditorRepository : IPageEditorRepository
    {
        private readonly IMongoCollection<PageEditorDocument> documents;

        public PageEditorRepository(IPagesDbContext dbContext)
        {
            documents = dbContext.ContentEditors;
        }

        public IQueryable<IPageEditor> ContentEditors => documents.AsQueryable();

        public async Task AssignEditorAsync(string email, CancellationToken cancellationToken = default)
        {
            await documents.InsertOneAsync(new PageEditorDocument
            {
                Id = ObjectId.GenerateNewId(),
                CreatedDate = DateTime.UtcNow,
                Version = 1,
                Email = email
            }, new InsertOneOptions(), cancellationToken);
        }

        public async Task<IPageEditor> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<PageEditorDocument>.Filter.Eq(it => it.Id, new ObjectId(id));
            var cursor = await documents.FindAsync(filter);
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<IPageEditor> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var filter = Builders<PageEditorDocument>.Filter.Eq(it => it.Email, email);
            var cursor = await documents.FindAsync(filter);
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task DeleteAsync(IPageEditor pageEditor, CancellationToken cancellationToken = default)
        {
            var pageEditorDocument = (PageEditorDocument)pageEditor;
            var curVersion = pageEditorDocument.Version;
            pageEditorDocument.Version++;

            var deleteResult = await documents.DeleteOneAsync(it => it.Id == pageEditorDocument.Id && it.Version == curVersion, cancellationToken: cancellationToken);
            if (deleteResult.DeletedCount != 1)
                throw new InvalidOperationException();
        }
    }
}