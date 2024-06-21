using System.Linq.Expressions;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageContentRepository : IContentEditRepository
    {
        private static readonly Expression<Func<ContentEditDocument, ContentEdit>> ProjectionExpression;
        readonly IMongoCollection<ContentEditDocument> documents;
        readonly IMongoCollection<ContentDocument> contentDocuments;

        static PageContentRepository()
        {
            ProjectionExpression = it => new ContentEdit
            {
                Id = it.Id,
                CreatedDate = it.CreatedDate,
                PageId = it.PageId,
                UserId = it.UserId
            };
        }

        public PageContentRepository(IPagesDbContext dbContext)
        {
            documents = dbContext.PageEditSessions;
            contentDocuments = dbContext.Contents;
        }

        public async Task<IContentEdit> CreateEditAsync(IPage page, string userId, CancellationToken cancellationToken = default)
        {
            var pageContent = await (await contentDocuments.FindAsync(it => it.PageId == page.Id, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);

            var createdDate = DateTime.UtcNow;
            var document = new ContentEditDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = createdDate,
                Version = 1,
                WebsiteId = page.WebsiteId,
                PageId = page.Id,
                UserId = userId,
                Content = pageContent.Data
            };

            await documents.InsertOneAsync(document, cancellationToken: cancellationToken);

            return new ContentEdit
            {
                Id = document.Id,
                CreatedDate = createdDate,
                PageId = document.PageId,
                UserId = document.UserId
            };
        }

        public async Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cursor = await documents.Find(it => it.Id == id).Project(ProjectionExpression).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IContentEdit> FindEditByUserAsync(IPage page, string userId, CancellationToken cancellationToken = default)
        {
            var cursor = await documents.Find(it => it.PageId == page.Id && it.UserId == userId).Project(ProjectionExpression).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IDictionary<string, object>> GetContentAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default)
        {
            var document = await (await documents.FindAsync(it => it.Id == pageEdit.Id, cancellationToken: cancellationToken)).FirstOrDefaultAsync(cancellationToken);
            if (document == null)
                return null;

            return MongoDbHelper.BsonDocumentToDictionary(document.Content);
        }

        public async Task SetContentAsync(IContentEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(contentData);
            var updateDefinition = Builders<ContentEditDocument>.Update.Set(it => it.Content, contentDataDocument);

            var updateResult = await documents.UpdateOneAsync(it => it.Id == pageEdit.Id, updateDefinition, cancellationToken: cancellationToken);
            if (updateResult.MatchedCount != 1)
                throw new InvalidOperationException();
        }

        public async Task DeleteEditAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default)
        {
            await documents.FindOneAndDeleteAsync(it => it.Id == pageEdit.Id, cancellationToken: cancellationToken);
        }
    }
}