using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageEditSessionRepository : MongoRepository<PageEditSessionDocument>, IPageEditSessionRepository
    {
        private static readonly Expression<Func<PageEditSessionDocument, PageEditSession>> ProjectionExpression;

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

        public PageEditSessionRepository(IPagesDbContext dbContext) : base(dbContext.PageEditSessions) { }

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

            await AddAsync(document);

            return new PageEditSession { Id = document.Id, PageId = document.PageId, ContentManagerId = document.ContentManagerId };
        }

        public async Task<IPageEditSession> FindEditSessionByIdAsync(Guid id)
        {
            var cursor = await mongoCollection.Find(it => it.Id == id)
                .Project(ProjectionExpression).ToCursorAsync();

            return await cursor.FirstOrDefaultAsync();
        }

        public async Task<PageContent> GetContentAsync(Guid sessionId)
        {
            var document = await (await mongoCollection.FindAsync(it => it.Id == sessionId)).FirstOrDefaultAsync();
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

            var updateResult = await mongoCollection.UpdateOneAsync(it => it.Id == sessionId, updateDefinition);

            if (updateResult.MatchedCount != 1)
                throw new InvalidOperationException();
        }

        public Task DeleteEditSession(Guid sessionId)
        {
            return DeleteAsync(sessionId);
        }
    }

    public static class MongoDbHelper
    {
        public static BsonDocument DictionaryToBsonDocument(IDictionary<string, object> dictionary)
        {
            return new BsonDocument(dictionary);
        }
        public static IDictionary<string, object> BsonDocumentToDictionary(BsonDocument document)
        {
            var result = new Dictionary<string, object>();

            foreach (var element in document.Elements)
            {
                if (element.Value.IsBsonArray)
                {
                    var list = new List<IDictionary<string, object>>();
                    foreach (var d in element.Value.AsBsonArray)
                        list.Add(BsonDocumentToDictionary(d.AsBsonDocument));
                    result.Add(element.Name, list);
                }
                else if (element.Value.IsBsonDocument)
                    result.Add(element.Name, BsonDocumentToDictionary(element.Value.AsBsonDocument));
                else
                    result.Add(element.Name, BsonTypeMapper.MapToDotNetValue(element.Value));
            }

            return result;
        }
    }
}