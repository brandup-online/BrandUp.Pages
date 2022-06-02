using BrandUp.Extensions.Migrations;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task UpAsync(CancellationToken cancellationToken = default)
        {
            await CreateIndexes_PageCollectionAsync();
            await CreateIndexes_PageAsync();
            await CreateIndexes_PageUrlAsync();
            await CreateIndexes_PageEditAsync();
            await CreateIndexes_PageRecyclebinAsync();
        }
        public Task DownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        #endregion

        async Task CreateIndexes_PageCollectionAsync()
        {
            var versionIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var pageIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.WebSiteId).Ascending(it => it.PageId);
            var pageTypeIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.WebSiteId).Ascending(it => it.PageTypeName);
            var titleIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.WebSiteId).Text(it => it.Title).Text(it => it.PageTypeName);

            await ApplyIndexes(dbContext.PageCollections, new CreateIndexModel<PageCollectionDocument>[] {
                new CreateIndexModel<PageCollectionDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageCollectionDocument>(pageIndex, new CreateIndexOptions { Name = "Page" }),
                new CreateIndexModel<PageCollectionDocument>(pageTypeIndex, new CreateIndexOptions { Name = "PageType" }),
                new CreateIndexModel<PageCollectionDocument>(titleIndex, new CreateIndexOptions { Name = "Title" })
            });
        }
        async Task CreateIndexes_PageAsync()
        {
            var versionIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var websiteIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebSiteId);
            var collectionIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.OwnCollectionId);
            var statusIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.Status);
            var urlIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebSiteId).Ascending(it => it.UrlPath);
            var textIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebSiteId).Text(it => it.Header).Text(it => it.Seo.Title).Text(it => it.Seo.Description).Text(it => it.UrlPath);

            await ApplyIndexes(dbContext.Pages, new CreateIndexModel<PageDocument>[] {
                new CreateIndexModel<PageDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageDocument>(websiteIndex, new CreateIndexOptions { Name = "Website" }),
                new CreateIndexModel<PageDocument>(collectionIndex, new CreateIndexOptions { Name = "Collection" }),
                new CreateIndexModel<PageDocument>(statusIndex, new CreateIndexOptions { Name = "Status" }),
                new CreateIndexModel<PageDocument>(urlIndex, new CreateIndexOptions { Name = "Url", Unique = true }),
                new CreateIndexModel<PageDocument>(textIndex, new CreateIndexOptions { Name = "Text" })
            });
        }
        async Task CreateIndexes_PageUrlAsync()
        {
            var pathIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.WebSiteId).Ascending(it => it.Path);
            var websiteIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.WebSiteId);
            var pageIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.PageId);

            await ApplyIndexes(dbContext.PageUrls, new CreateIndexModel<PageUrlDocument>[] {
                new CreateIndexModel<PageUrlDocument>(pathIndex, new CreateIndexOptions { Name = "Path" }),
                new CreateIndexModel<PageUrlDocument>(websiteIndex, new CreateIndexOptions { Name = "Website" }),
                new CreateIndexModel<PageUrlDocument>(pageIndex, new CreateIndexOptions { Name = "Page" })
            });
        }
        async Task CreateIndexes_PageEditAsync()
        {
            var versionIndex = Builders<PageEditDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var userIndex = Builders<PageEditDocument>.IndexKeys.Ascending(it => it.PageId).Ascending(it => it.UserId);

            await ApplyIndexes(dbContext.PageEditSessions, new CreateIndexModel<PageEditDocument>[] {
                new CreateIndexModel<PageEditDocument>(versionIndex, new CreateIndexOptions { Name = "Version" }),
                new CreateIndexModel<PageEditDocument>(userIndex, new CreateIndexOptions { Name = "User" })
            });
        }
        async Task CreateIndexes_PageRecyclebinAsync()
        {
            var versionIndex = Builders<PageRecyclebinDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);

            await ApplyIndexes(dbContext.PageRecyclebin, new CreateIndexModel<PageRecyclebinDocument>[] {
                new CreateIndexModel<PageRecyclebinDocument>(versionIndex, new CreateIndexOptions { Name = "Version" })
            });
        }

        #region Helpers

        private async Task ApplyIndexes<TDocument>(IMongoCollection<TDocument> collection, IEnumerable<CreateIndexModel<TDocument>> indexes)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (indexes == null)
                throw new ArgumentNullException(nameof(indexes));

            var currentIndexNames = (await (await collection.Indexes.ListAsync(new ListIndexesOptions())).ToListAsync()).Select(it => it["name"].AsString);
            foreach (var index in indexes)
            {
                var indexName = index.Options.Name;
                if (currentIndexNames.Contains(indexName))
                    await collection.Indexes.DropOneAsync(indexName);
            }

            await collection.Indexes.CreateManyAsync(indexes);
        }

        #endregion
    }
}