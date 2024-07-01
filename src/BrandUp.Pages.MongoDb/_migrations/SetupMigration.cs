using BrandUp.Extensions.Migrations;
using BrandUp.MongoDB;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb._migrations
{
    [Setup]
    public class SetupMigration(IPagesDbContext dbContext) : IMigrationHandler
    {
        readonly IPagesDbContext dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        #region IMigrationHandler members

        async Task IMigrationHandler.UpAsync(CancellationToken cancellationToken)
        {
            await CreateIndexes_PageCollectionAsync(cancellationToken);
            await CreateIndexes_PageAsync(cancellationToken);
            await CreateIndexes_ContentAsync(cancellationToken);
            await CreateIndexes_PageUrlAsync(cancellationToken);
            await CreateIndexes_ContentEditsAsync(cancellationToken);
            await CreateIndexes_PageRecyclebinAsync(cancellationToken);
        }
        Task IMigrationHandler.DownAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion

        async Task CreateIndexes_PageCollectionAsync(CancellationToken cancellationToken)
        {
            var versionIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var pageIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.PageId);
            var pageTypeIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.PageTypeName);
            var titleIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.WebsiteId).Text(it => it.Title).Text(it => it.PageTypeName);

            await dbContext.PageCollections.Indexes.ApplyIndexes([
                new CreateIndexModel<PageCollectionDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageCollectionDocument>(pageIndex, new CreateIndexOptions { Name = "Page" }),
                new CreateIndexModel<PageCollectionDocument>(pageTypeIndex, new CreateIndexOptions { Name = "PageType" }),
                new CreateIndexModel<PageCollectionDocument>(titleIndex, new CreateIndexOptions { Name = "Title" })
            ], true, cancellationToken);
        }

        async Task CreateIndexes_PageAsync(CancellationToken cancellationToken)
        {
            var versionIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var websiteIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebsiteId);
            var collectionIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.OwnCollectionId);
            var statusIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.Status);
            var urlIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.UrlPath);
            var textIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebsiteId).Text(it => it.Header).Text(it => it.Seo.Title).Text(it => it.Seo.Description).Text(it => it.UrlPath);

            await dbContext.Pages.Indexes.ApplyIndexes([
                new CreateIndexModel<PageDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageDocument>(websiteIndex, new CreateIndexOptions { Name = "Website" }),
                new CreateIndexModel<PageDocument>(collectionIndex, new CreateIndexOptions { Name = "Collection" }),
                new CreateIndexModel<PageDocument>(statusIndex, new CreateIndexOptions { Name = "Status" }),
                new CreateIndexModel<PageDocument>(urlIndex, new CreateIndexOptions { Name = "Url", Unique = true }),
                new CreateIndexModel<PageDocument>(textIndex, new CreateIndexOptions { Name = "Text" })
            ], true, cancellationToken);
        }

        async Task CreateIndexes_PageUrlAsync(CancellationToken cancellationToken)
        {
            var pathIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.Path);
            var websiteIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.WebsiteId);
            var pageIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.PageId);

            await dbContext.PageUrls.Indexes.ApplyIndexes([
                new CreateIndexModel<PageUrlDocument>(pathIndex, new CreateIndexOptions { Name = "Path" }),
                new CreateIndexModel<PageUrlDocument>(websiteIndex, new CreateIndexOptions { Name = "Website" }),
                new CreateIndexModel<PageUrlDocument>(pageIndex, new CreateIndexOptions { Name = "Page" })
            ], true, cancellationToken);
        }

        async Task CreateIndexes_PageRecyclebinAsync(CancellationToken cancellationToken)
        {
            var versionIndex = Builders<PageRecyclebinDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var websiteIndex = Builders<PageRecyclebinDocument>.IndexKeys.Ascending(it => it.WebsiteId);

            await dbContext.PageRecyclebin.Indexes.ApplyIndexes([
                new CreateIndexModel<PageRecyclebinDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageRecyclebinDocument>(websiteIndex, new CreateIndexOptions { Name = "Website" })
            ], true, cancellationToken);
        }

        async Task CreateIndexes_ContentAsync(CancellationToken cancellationToken)
        {
            var keyIndex = Builders<ContentDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.Key);

            await dbContext.Contents.Indexes.ApplyIndexes([
                new CreateIndexModel<ContentDocument>(keyIndex, new CreateIndexOptions { Name = "Key", Unique = true })
            ], true, cancellationToken);
        }

        async Task CreateIndexes_ContentEditsAsync(CancellationToken cancellationToken)
        {
            var versionIndex = Builders<ContentEditDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var userIndex = Builders<ContentEditDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.ContentKey).Ascending(it => it.UserId);

            await dbContext.ContentEdits.Indexes.ApplyIndexes([
                new CreateIndexModel<ContentEditDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<ContentEditDocument>(userIndex, new CreateIndexOptions { Name = "User", Unique = true })
            ], true, cancellationToken);
        }
    }
}