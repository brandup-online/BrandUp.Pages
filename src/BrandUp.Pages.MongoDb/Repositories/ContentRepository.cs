using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Repositories;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class ContentRepository(IPagesDbContext dbContext) : IContentRepository
    {
        readonly IMongoCollection<ContentDocument> contentDocuments = dbContext.Contents;

        public async Task<IContent> FindByKeyAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            key = NormalizeAndValidateKey(key);

            var content = await contentDocuments
                .Find(it => it.WebsiteId == websiteId && it.Key == key)
                .Project(Builders<ContentDocument>.Projection.As<Content>())
                .SingleOrDefaultAsync(cancellationToken);

            return content;
        }

        public async Task<ContentDataResult> GetDataAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);

            key = NormalizeAndValidateKey(key);

            var contentData = await contentDocuments
                .Find(it => it.WebsiteId == websiteId && it.Key == key)
                .Project(Builders<ContentDocument>.Projection.As<ContentDataProject>())
                .SingleOrDefaultAsync(cancellationToken);
            if (contentData == null)
                return null;

            return new ContentDataResult
            {
                Type = contentData.Type,
                Data = MongoDbHelper.BsonDocumentToDictionary(contentData.Data),
                Version = contentData.Version
            };
        }

        public async Task<ContentDataResult> SetDataAsync(string websiteId, string key, string prevVersion, string type, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            key = NormalizeAndValidateKey(key);

            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(contentData);

            var updateDef = Builders<ContentDocument>.Update
                    .Set(it => it.Type, type)
                    .Set(it => it.Title, title)
                    .Set(it => it.Version, ObjectId.GenerateNewId())
                    .Set(it => it.Data, contentDataDocument);

            if (prevVersion != null)
                updateDef.Set(it => it.Prev, ObjectId.Parse(prevVersion));

            var contentDocument = await contentDocuments.FindOneAndUpdateAsync<ContentDocument>(
                it => it.WebsiteId == websiteId && it.Key == key,
                updateDef,
                new FindOneAndUpdateOptions<ContentDocument, ContentDocument> { IsUpsert = true, ReturnDocument = ReturnDocument.After },
                cancellationToken: cancellationToken);

            return new ContentDataResult
            {
                Type = contentDocument.Type,
                Data = MongoDbHelper.BsonDocumentToDictionary(contentDocument.Data),
                Version = contentDocument.Version.ToString()
            };
        }

        static string NormalizeAndValidateKey(string key)
        {
            var result = key.Trim().ToLower();
            if (string.IsNullOrEmpty(result))
                throw new InvalidOperationException($"Invalid key \"{key}\".");
            return result;
        }

        class Content : IContent
        {
            public Guid Id { get; set; }
            public string WebsiteId { get; set; }
            public string Key { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public string Version { get; set; }
        }

        class ContentDataProject
        {
            public string Type { get; set; }
            public BsonDocument Data { get; set; }
            public string Version { get; set; }
        }
    }
}