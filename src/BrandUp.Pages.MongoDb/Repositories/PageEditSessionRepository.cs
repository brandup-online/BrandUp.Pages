using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageEditSessionRepository : IPageEditSessionRepository
    {
        private static readonly Expression<Func<PageEditSessionDocument, PageEditSession>> ProjectionExpression;
        readonly IMongoCollection<PageEditSessionDocument> documents;

        static PageEditSessionRepository()
        {
            ProjectionExpression = it => new PageEditSession
            {
                Id = it.Id,
                CreatedDate = it.CreatedDate,
                PageId = it.PageId,
                ContentManagerId = it.ContentManagerId
            };
        }

        public PageEditSessionRepository(IPagesDbContext dbContext)
        {
            documents = dbContext.PageEditSessions;
        }

        public async Task<IPageEditSession> CreateEditSessionAsync(Guid pageId, string contentManagerId, PageContent content)
        {
            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(content.Data);

            var document = new PageEditSessionDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                PageId = pageId,
                ContentManagerId = contentManagerId,
                Content = contentDataDocument
            };

            await documents.InsertOneAsync(document);

            return new PageEditSession { Id = document.Id, PageId = document.PageId, ContentManagerId = document.ContentManagerId };
        }

        public async Task<IPageEditSession> FindEditSessionByIdAsync(Guid id)
        {
            var cursor = await documents.Find(it => it.Id == id)
                .Project(ProjectionExpression).ToCursorAsync();

            return await cursor.FirstOrDefaultAsync();
        }

        public async Task<PageContent> GetContentAsync(Guid sessionId)
        {
            var document = await (await documents.FindAsync(it => it.Id == sessionId)).FirstOrDefaultAsync();
            if (document == null)
                return null;

            return new PageContent(1, MongoDbHelper.BsonDocumentToDictionary(document.Content));
        }

        public async Task SetContentAsync(Guid sessionId, PageContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(content.Data);
            var updateDefinition = Builders<PageEditSessionDocument>.Update.Set(it => it.Content, contentDataDocument);

            var updateResult = await documents.UpdateOneAsync(it => it.Id == sessionId, updateDefinition);

            if (updateResult.MatchedCount != 1)
                throw new InvalidOperationException();
        }

        public async Task DeleteEditSessionAsync(Guid sessionId)
        {
            var filter = Builders<PageEditSessionDocument>.Filter.Eq(it => it.Id, sessionId);
            await documents.FindOneAndDeleteAsync(filter);
        }
    }
}