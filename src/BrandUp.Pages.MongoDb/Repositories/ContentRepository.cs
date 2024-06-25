using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class ContentRepository(IPagesDbContext dbContext) : IContentRepository
    {
        readonly IMongoCollection<ContentDocument> contentDocuments = dbContext.Contents;

        public async Task<IDictionary<string, object>> GetContentAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);

            key = NormalizeAndValidateKey(key);

            var content = await (await contentDocuments.FindAsync(it => it.WebsiteId == websiteId && it.Key == key, cancellationToken: cancellationToken)).FirstOrDefaultAsync(cancellationToken);
            if (content == null)
                return null;

            return MongoDbHelper.BsonDocumentToDictionary(content.Data);
        }

        public async Task SetContentAsync(string websiteId, string key, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);

            key = NormalizeAndValidateKey(key);

            if (contentData != null)
            {
                var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(contentData);

                var contentUpdateResult = await contentDocuments.UpdateOneAsync(it => it.WebsiteId == websiteId && it.Key == key, Builders<ContentDocument>.Update
                        .Set(it => it.Data, contentDataDocument), cancellationToken: cancellationToken);
                if (contentUpdateResult.MatchedCount != 1)
                    await contentDocuments.InsertOneAsync(new ContentDocument { WebsiteId = websiteId, Key = key, Data = contentDataDocument }, cancellationToken: cancellationToken);
            }
            else
                await contentDocuments.DeleteOneAsync(it => it.WebsiteId == websiteId && it.Key == key, cancellationToken: cancellationToken);
        }

        static string NormalizeAndValidateKey(string key)
        {
            var result = key.Trim().ToLower();
            if (string.IsNullOrEmpty(result))
                throw new InvalidOperationException($"Invalid key \"{key}\".");
            return result;
        }
    }
}