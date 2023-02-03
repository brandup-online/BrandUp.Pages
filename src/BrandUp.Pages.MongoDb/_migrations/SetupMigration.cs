using BrandUp.Extensions.Migrations;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb._migrations
{
    [Setup]
    public class SetupMigration : IMigrationHandler
    {
        readonly IPagesDbContext dbContext;

        public SetupMigration(IPagesDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        #region IMigrationHandler members

        async Task IMigrationHandler.UpAsync(CancellationToken cancellationToken)
        {
            await CreateIndexes_PageCollectionAsync(cancellationToken);
            await CreateIndexes_PageAsync(cancellationToken);
            await CreateIndexes_PageContentAsync(cancellationToken);
            await CreateIndexes_PageUrlAsync(cancellationToken);
            await CreateIndexes_PageEditAsync(cancellationToken);
            await CreateIndexes_PageRecyclebinAsync(cancellationToken);
        }

        Task IMigrationHandler.DownAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion

        async Task CreateIndexes_PageCollectionAsync(CancellationToken cancellationToken)
        {
            await dbContext.PageCollections.Indexes.DropAllAsync(cancellationToken);

            var versionIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var pageIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.PageId);
            var pageTypeIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.PageTypeName);
            var titleIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.WebsiteId).Text(it => it.Title).Text(it => it.PageTypeName);

            await ApplyIndexes(dbContext.PageCollections, new CreateIndexModel<PageCollectionDocument>[] {
                new CreateIndexModel<PageCollectionDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageCollectionDocument>(pageIndex, new CreateIndexOptions { Name = "Page" }),
                new CreateIndexModel<PageCollectionDocument>(pageTypeIndex, new CreateIndexOptions { Name = "PageType" }),
                new CreateIndexModel<PageCollectionDocument>(titleIndex, new CreateIndexOptions { Name = "Title" })
            }, cancellationToken);
        }
        async Task CreateIndexes_PageAsync(CancellationToken cancellationToken)
        {
            await dbContext.Pages.Indexes.DropAllAsync(cancellationToken);

            var versionIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var websiteIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebsiteId);
            var collectionIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.OwnCollectionId);
            var statusIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.Status);
            var urlIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.UrlPath);
            var textIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebsiteId).Text(it => it.Header).Text(it => it.Seo.Title).Text(it => it.Seo.Description).Text(it => it.UrlPath);

            await ApplyIndexes(dbContext.Pages, new CreateIndexModel<PageDocument>[] {
                new CreateIndexModel<PageDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageDocument>(websiteIndex, new CreateIndexOptions { Name = "Website" }),
                new CreateIndexModel<PageDocument>(collectionIndex, new CreateIndexOptions { Name = "Collection" }),
                new CreateIndexModel<PageDocument>(statusIndex, new CreateIndexOptions { Name = "Status" }),
                new CreateIndexModel<PageDocument>(urlIndex, new CreateIndexOptions { Name = "Url", Unique = true }),
                new CreateIndexModel<PageDocument>(textIndex, new CreateIndexOptions { Name = "Text" })
            }, cancellationToken);
        }
        async Task CreateIndexes_PageContentAsync(CancellationToken cancellationToken)
        {
            await dbContext.PageEditSessions.Indexes.DropAllAsync(cancellationToken);

            var pageIdIndex = Builders<PageContentDocument>.IndexKeys.Ascending(it => it.PageId);

            await ApplyIndexes(dbContext.Contents, new CreateIndexModel<PageContentDocument>[] {
                new CreateIndexModel<PageContentDocument>(pageIdIndex, new CreateIndexOptions { Name = "Page", Unique = true })
            }, cancellationToken);
        }
        async Task CreateIndexes_PageUrlAsync(CancellationToken cancellationToken)
        {
            await dbContext.PageUrls.Indexes.DropAllAsync(cancellationToken);

            var pathIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.WebsiteId).Ascending(it => it.Path);
            var websiteIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.WebsiteId);
            var pageIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.PageId);

            await ApplyIndexes(dbContext.PageUrls, new CreateIndexModel<PageUrlDocument>[] {
                new CreateIndexModel<PageUrlDocument>(pathIndex, new CreateIndexOptions { Name = "Path" }),
                new CreateIndexModel<PageUrlDocument>(websiteIndex, new CreateIndexOptions { Name = "Website" }),
                new CreateIndexModel<PageUrlDocument>(pageIndex, new CreateIndexOptions { Name = "Page" })
            }, cancellationToken);
        }
        async Task CreateIndexes_PageEditAsync(CancellationToken cancellationToken)
        {
            await dbContext.PageEditSessions.Indexes.DropAllAsync(cancellationToken);

            var versionIndex = Builders<PageEditDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var userIndex = Builders<PageEditDocument>.IndexKeys.Ascending(it => it.PageId).Ascending(it => it.UserId);
            var websiteIndex = Builders<PageEditDocument>.IndexKeys.Ascending(it => it.WebsiteId);

            await ApplyIndexes(dbContext.PageEditSessions, new CreateIndexModel<PageEditDocument>[] {
                new CreateIndexModel<PageEditDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageEditDocument>(userIndex, new CreateIndexOptions { Name = "User" }),
                new CreateIndexModel<PageEditDocument>(websiteIndex, new CreateIndexOptions { Name = "Website" })
            }, cancellationToken);
        }
        async Task CreateIndexes_PageRecyclebinAsync(CancellationToken cancellationToken)
        {
            await dbContext.PageRecyclebin.Indexes.DropAllAsync(cancellationToken);

            var versionIndex = Builders<PageRecyclebinDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var websiteIndex = Builders<PageRecyclebinDocument>.IndexKeys.Ascending(it => it.WebsiteId);

            await ApplyIndexes(dbContext.PageRecyclebin, new CreateIndexModel<PageRecyclebinDocument>[] {
                new CreateIndexModel<PageRecyclebinDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageRecyclebinDocument>(websiteIndex, new CreateIndexOptions { Name = "Website" })
            }, cancellationToken);
        }

        #region Helpers

        static async Task ApplyIndexes<TDocument>(IMongoCollection<TDocument> collection, IEnumerable<CreateIndexModel<TDocument>> indexes, CancellationToken cancellationToken)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (indexes == null)
                throw new ArgumentNullException(nameof(indexes));

            var currentIndexNames = (await (await collection.Indexes.ListAsync(new ListIndexesOptions(), cancellationToken)).ToListAsync(cancellationToken)).Select(it => it["name"].AsString);
            foreach (var index in indexes)
            {
                var indexName = index.Options.Name;
                if (currentIndexNames.Contains(indexName))
                    await collection.Indexes.DropOneAsync(indexName, cancellationToken);
            }

            await collection.Indexes.CreateManyAsync(indexes, cancellationToken);
        }

        #endregion
    }
}