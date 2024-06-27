using System.Linq.Expressions;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Repositories;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class ContentEditRepository(IPagesDbContext dbContext) : IContentEditRepository
    {
        static readonly Expression<Func<ContentEditDocument, ContentEdit>> ProjectionExpression;
        readonly IMongoCollection<ContentEditDocument> documents = dbContext.ContentEdits;

        static ContentEditRepository()
        {
            ProjectionExpression = it => new ContentEdit
            {
                Id = it.Id,
                CreatedDate = it.CreatedDate,
                WebsiteId = it.WebsiteId,
                ContentKey = it.ContentKey,
                UserId = it.UserId
            };
        }

        public async Task<IContentEdit> CreateEditAsync(string websiteId, string contentKey, string sourceVersion, string userId, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(contentData);

            var createdDate = DateTime.UtcNow;
            var document = new ContentEditDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = createdDate,
                Version = 1,
                WebsiteId = websiteId,
                ContentKey = contentKey,
                ContentVersion = sourceVersion != null ? ObjectId.Parse(sourceVersion) : null,
                UserId = userId,
                Content = contentDataDocument
            };

            await documents.InsertOneAsync(document, cancellationToken: cancellationToken);

            return new ContentEdit
            {
                Id = document.Id,
                CreatedDate = createdDate,
                WebsiteId = document.WebsiteId,
                ContentKey = document.ContentKey,
                SourceCommitId = document.ContentVersion != null ? document.ContentVersion.ToString() : null,
                UserId = document.UserId
            };
        }

        public async Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cursor = await documents.Find(it => it.Id == id).Project(ProjectionExpression).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IContentEdit> FindEditByUserAsync(string websiteId, string contentKey, string userId, CancellationToken cancellationToken = default)
        {
            var cursor = await documents.Find(it => it.WebsiteId == websiteId && it.ContentKey == contentKey && it.UserId == userId).Project(ProjectionExpression).ToCursorAsync(cancellationToken);

            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IDictionary<string, object>> GetContentAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default)
        {
            var document = await (await documents.FindAsync(it => it.Id == pageEdit.Id, cancellationToken: cancellationToken)).FirstOrDefaultAsync(cancellationToken);
            if (document == null)
                return null;

            return MongoDbHelper.BsonDocumentToDictionary(document.Content);
        }

        public async Task UpdateContentAsync(IContentEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
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