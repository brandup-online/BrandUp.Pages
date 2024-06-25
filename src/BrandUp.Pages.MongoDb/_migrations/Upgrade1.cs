using BrandUp.Extensions.Migrations;
using BrandUp.MongoDB;
using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb._migrations
{
    [Upgrade(typeof(SetupMigration), Description = "New content structure.")]
    public class Upgrade1(IPagesDbContext dbContext, IPageService pageService, ContentMetadataManager contentMetadataManager, ILogger<Upgrade1> logger) : IMigrationHandler
    {
        #region IMigrationHandler members

        async Task IMigrationHandler.UpAsync(CancellationToken cancellationToken)
        {
            #region Contents

            // Delete old index
            await dbContext.Contents.Indexes.DropIfExistAsync("Page", cancellationToken: cancellationToken);

            #region Remove pageId

            var prevContents = await (await dbContext.Contents
                .Find(Builders<ContentDocument>.Filter.Exists("pageId", true))
                .Project<PrevContent>(Builders<ContentDocument>.Projection.Include("_id").Include("pageId"))
                .ToCursorAsync(cancellationToken)).ToListAsync(cancellationToken);
            foreach (var prev in prevContents)
            {
                if (!prev.PageId.HasValue)
                    continue;

                var page = await (await dbContext.Pages.FindAsync(it => it.Id == prev.PageId.Value, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
                if (page == null)
                {
                    logger.LogWarning($"Not found page by ID {prev.PageId.Value}.");
                    continue;
                }

                var contentKey = await pageService.GetContentKeyAsync(page.Id, cancellationToken);

                var updateResult = await dbContext.Contents.UpdateOneAsync(
                        it => it.Id == prev.Id,
                        Builders<ContentDocument>.Update
                            .Unset("pageId")
                            .Set(it => it.WebsiteId, page.WebsiteId)
                            .Set(it => it.Key, contentKey),
                        new UpdateOptions { IsUpsert = false }, cancellationToken
                    );
                if (updateResult.ModifiedCount != 1)
                    throw new InvalidOperationException($"Unable to update content {prev.Id}.");
            }

            #endregion

            #region Add properties

            var prevContentsCursor2 = await (await dbContext.Contents
                .Find(Builders<ContentDocument>.Filter.Exists("type", false))
                .Project<PrevContent2>(Builders<ContentDocument>.Projection.Include("_id").Include("data"))
                .ToCursorAsync(cancellationToken)).ToListAsync(cancellationToken);
            foreach (var prev in prevContentsCursor2)
            {
                var contentData = MongoDbHelper.BsonDocumentToDictionary(prev.Data);
                var contentMetadata = contentMetadataManager.GetMetadata(contentData);
                var contentModel = contentMetadataManager.ConvertDictionaryToContentModel(contentData);
                var contentTitle = contentMetadata.GetContentTitle(contentModel);

                var updateResult = await dbContext.Contents.UpdateOneAsync(
                        it => it.Id == prev.Id,
                        Builders<ContentDocument>.Update
                            .Set(it => it.Type, contentMetadata.Name)
                            .Set(it => it.Title, contentTitle)
                            .Set(it => it.Version, Guid.NewGuid()),
                        new UpdateOptions { IsUpsert = false }, cancellationToken
                    );
                if (updateResult.ModifiedCount != 1)
                    throw new InvalidOperationException($"Unable to update content {prev.Id}.");
            }

            // Create new index
            var keyIndex = Builders<ContentDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.Key);
            await dbContext.Contents.Indexes.ApplyIndexes([
                new CreateIndexModel<ContentDocument>(keyIndex, new CreateIndexOptions { Name = "Key", Unique = true })
            ], true, cancellationToken);

            #endregion

            #endregion

            #region Content edits

            // Delete all edit sessions by pageId
            await dbContext.ContentEdits.DeleteManyAsync(Builders<ContentEditDocument>.Filter.Exists("pageId", true), cancellationToken);

            await dbContext.ContentEdits.Indexes.DropIfExistAsync("Website", cancellationToken: cancellationToken);

            // Recreate edit index
            var userIndex = Builders<ContentEditDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.ContentKey).Ascending(it => it.UserId);
            await dbContext.ContentEdits.Indexes.ApplyIndexes([
                new CreateIndexModel<ContentEditDocument>(userIndex, new CreateIndexOptions { Name = "User", Unique = true })
            ], true, cancellationToken);

            #endregion
        }

        Task IMigrationHandler.DownAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion

        class PrevContent
        {
            [BsonElement("_id")]
            public Guid Id { get; set; }
            [BsonElement("pageId")]
            public Guid? PageId { get; set; }
        }

        class PrevContent2
        {
            [BsonElement("_id")]
            public Guid Id { get; set; }
            [BsonElement("data")]
            public BsonDocument Data { get; set; }
        }
    }
}