using BrandUp.MongoDB;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Repositories;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class ContentRepository(IPagesDbContext dbContext, MongoDbSession mongoDbSession) : IContentRepository
    {
        readonly IMongoCollection<ContentDocument> contents = dbContext.Contents;
        readonly IMongoCollection<ContentCommitDocument> commits = dbContext.ContentCommits;

        public async Task<IContent> FindByKeyAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            key = NormalizeAndValidateKey(key);

            var content = await contents
                .Find(it => it.WebsiteId == websiteId && it.Key == key)
                .Project(Builders<ContentDocument>.Projection.As<Content>())
                .SingleOrDefaultAsync(cancellationToken);

            return content;
        }

        public async Task<ContentCommitResult> FindCommitByIdAsync(string commitId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(commitId);

            var id = ObjectId.Parse(commitId);

            var contentData = await commits
                .Find(it => it.Id == id)
                .Project(Builders<ContentCommitDocument>.Projection.As<ContentCommitProject>())
                .SingleOrDefaultAsync(cancellationToken);
            if (contentData == null)
                return null;

            return new ContentCommitResult
            {
                Type = contentData.Type,
                Title = contentData.Title,
                Data = MongoDbHelper.BsonDocumentToDictionary(contentData.Data),
                CommitId = contentData.Id
            };
        }

        public async Task<Guid> CreateContentAsync(string websiteId, string key, CancellationToken cancellationToken)
        {
            var content = new ContentDocument
            {
                Id = Guid.NewGuid(),
                WebsiteId = websiteId,
                Key = key,
                CommitId = null
            };

            await contents.InsertOneAsync(content, cancellationToken: cancellationToken);

            return content.Id;
        }

        public async Task CreateCommitAsync(Guid contentId, string sourceCommitId, string userId, string type, IDictionary<string, object> data, string title, CancellationToken cancellationToken)
        {
            using var transaction = await mongoDbSession.BeginAsync(cancellationToken);

            var commit = new ContentCommitDocument
            {
                Id = ObjectId.GenerateNewId(),
                SourceId = sourceCommitId != null ? ObjectId.Parse(sourceCommitId) : null,
                ContentId = contentId,
                Date = DateTime.UtcNow,
                UserId = userId,
                Type = type,
                Title = title,
                Data = MongoDbHelper.DictionaryToBsonDocument(data)
            };
            await commits.InsertOneAsync(mongoDbSession.Current, commit, cancellationToken: cancellationToken);

            var updateResult = await contents.UpdateOneAsync(
                mongoDbSession.Current,
                it => it.Id == contentId,
                Builders<ContentDocument>.Update
                    .Set(it => it.CommitId, commit.Id),
                new UpdateOptions { IsUpsert = false, BypassDocumentValidation = false }, cancellationToken);
            if (updateResult.ModifiedCount != 1)
                throw new InvalidOperationException("Не удалось обновить контент.");

            await transaction.CommitAsync(cancellationToken);
        }

        public async Task<ContentCommitResult> SetDataAsync(string websiteId, string key, string prevVersion, string type, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            key = NormalizeAndValidateKey(key);

            var contentDataDocument = MongoDbHelper.DictionaryToBsonDocument(contentData);

            var updateDef = Builders<ContentDocument>.Update
                    .Set(it => it.Type, type)
                    .Set(it => it.Title, title)
                    .Set(it => it.CommitId, ObjectId.GenerateNewId())
                    .Set(it => it.Data, contentDataDocument);

            var contentDocument = await contents.FindOneAndUpdateAsync<ContentDocument>(
                it => it.WebsiteId == websiteId && it.Key == key,
                updateDef,
                new FindOneAndUpdateOptions<ContentDocument, ContentDocument> { IsUpsert = true, ReturnDocument = ReturnDocument.After },
                cancellationToken: cancellationToken);

            return new ContentCommitResult
            {
                Type = contentDocument.Type,
                Data = MongoDbHelper.BsonDocumentToDictionary(contentDocument.Data),
                CommitId = contentDocument.CommitId.ToString()
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
            public string CommitId { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
        }

        class ContentCommitProject
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public BsonDocument Data { get; set; }
        }
    }
}