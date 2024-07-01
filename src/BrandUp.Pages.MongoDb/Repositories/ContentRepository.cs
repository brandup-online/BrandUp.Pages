using System.Linq.Expressions;
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

        #region IContentRepository members

        public async Task<IContent> FindByIdAsync(Guid contentId, CancellationToken cancellationToken = default)
        {
            var content = await contents
                .Find(it => it.Id == contentId)
                .SingleOrDefaultAsync(cancellationToken);

            return content;
        }

        public async Task<IContent> FindByKeyAsync(string contentKey, CancellationToken cancellationToken = default)
        {
            contentKey = NormalizeAndValidateKey(contentKey);

            var content = await contents
                .Find(it => it.Key == contentKey)
                .SingleOrDefaultAsync(cancellationToken);

            return content;
        }

        public async Task<IContent> CreateContentAsync(string contentKey, CancellationToken cancellationToken)
        {
            contentKey = NormalizeAndValidateKey(contentKey);

            var content = new ContentDocument
            {
                Id = Guid.NewGuid(),
                Key = contentKey,
                CommitId = null
            };

            await contents.InsertOneAsync(content, cancellationToken: cancellationToken);

            return content;
        }

        public async Task<ContentCommitResult> FindCommitByIdAsync(string commitId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(commitId);

            var id = ObjectId.Parse(commitId);

            var contentData = await commits
                .Find(it => it.Id == id)
                .Project(ContentCommitProject.Projection)
                .SingleOrDefaultAsync(cancellationToken);
            if (contentData == null)
                return null;

            return new ContentCommitResult
            {
                Type = contentData.Type,
                Title = contentData.Title,
                Data = MongoDbHelper.BsonDocumentToDictionary(contentData.Data),
                CommitId = contentData.Id.ToString()
            };
        }

        public async Task<ContentCommitResult> CreateCommitAsync(Guid contentId, string sourceCommitId, string userId, string type, IDictionary<string, object> data, string title, CancellationToken cancellationToken)
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

            return new ContentCommitResult
            {
                Type = commit.Type,
                Data = MongoDbHelper.BsonDocumentToDictionary(commit.Data),
                CommitId = commit.Id.ToString()
            };
        }

        public async Task DeleteAsync(Guid contentId, CancellationToken cancellationToken = default)
        {
            using var transaction = await mongoDbSession.BeginAsync(cancellationToken);

            var deleteContentResult = await contents.DeleteOneAsync(mongoDbSession.Current, it => it.Id == contentId, cancellationToken: cancellationToken);
            if (deleteContentResult.DeletedCount != 1)
                throw new InvalidOperationException($"Unable to delete content by ID {contentId}.");

            await commits.DeleteManyAsync(mongoDbSession.Current, it => it.ContentId == contentId, cancellationToken: cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        #endregion

        #region Helpers

        static string NormalizeAndValidateKey(string key)
        {
            if (key == null)
                ArgumentNullException.ThrowIfNull(key);

            var result = key.Trim().ToLower();
            if (string.IsNullOrEmpty(result))
                throw new InvalidOperationException($"Invalid key \"{key}\".");
            return result;
        }

        #endregion

        class ContentCommitProject
        {
            public static readonly Expression<Func<ContentCommitDocument, ContentCommitProject>> Projection = it => new ContentCommitProject
            {
                Id = it.Id,
                Type = it.Type,
                Title = it.Title,
                Data = it.Data
            };

            public ObjectId Id { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public BsonDocument Data { get; set; }
        }
    }
}