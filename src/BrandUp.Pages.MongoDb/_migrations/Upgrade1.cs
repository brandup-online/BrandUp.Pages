using BrandUp.Extensions.Migrations;
using BrandUp.MongoDB;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb._migrations
{
    [Upgrade(typeof(SetupMigration), Description = "New content structure.")]
    public class Upgrade1(IPagesDbContext dbContext, IPageService pageService, ILogger<Upgrade1> logger) : IMigrationHandler
    {
        #region IMigrationHandler members

        async Task IMigrationHandler.UpAsync(CancellationToken cancellationToken)
        {
            #region Contents

            // Delete old index
            await dbContext.Contents.Indexes.DropIfExistAsync("Page", cancellationToken: cancellationToken);

            #region Upgrade document structure

#pragma warning disable CS0612 // Type or member is obsolete
            var prevContentsCursor = await dbContext.Contents
                .Find(Builders<ContentDocument>.Filter.Exists(it => it.PageId, true))
                .Project(it => new PrevContent { Id = it.Id, PageId = it.PageId })
                .ToCursorAsync(cancellationToken);
#pragma warning restore CS0612 // Type or member is obsolete

            var prevContents = await prevContentsCursor.ToListAsync(cancellationToken);
            foreach (var prev in prevContents)
            {
                var page = await (await dbContext.Pages.FindAsync(it => it.Id == prev.PageId.Value, cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
                if (page == null)
                {
                    logger.LogWarning($"Not found page by ID {prev.PageId.Value}.");
                    continue;
                }

                var contentKey = await pageService.GetContentKeyAsync(page.Id, cancellationToken);

#pragma warning disable CS0612 // Type or member is obsolete
                var updateResult = await dbContext.Contents.UpdateOneAsync(
                        it => it.Id == prev.Id,
                        Builders<ContentDocument>.Update.Unset(it => it.PageId).Set(it => it.WebsiteId, page.WebsiteId).Set(it => it.Key, contentKey),
                        new UpdateOptions { IsUpsert = false }, cancellationToken
                    );
#pragma warning restore CS0612 // Type or member is obsolete

                if (updateResult.ModifiedCount != 1)
                    throw new InvalidOperationException($"Unable to update content {prev.Id}.");
            }

            #endregion

            // Create new index
            var keyIndex = Builders<ContentDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.Key);
            await dbContext.Contents.Indexes.ApplyIndexes([
                new CreateIndexModel<ContentDocument>(keyIndex, new CreateIndexOptions { Name = "Key", Unique = true })
            ], true, cancellationToken);

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
            public Guid Id { get; set; }
            public Guid? PageId { get; set; }
        }
    }
}