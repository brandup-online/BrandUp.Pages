using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using BrandUp.Pages.Services;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageCollectionRepository : IPageCollectionRepository
    {
        readonly IMongoCollection<PageCollectionDocument> documents;

        public PageCollectionRepository(IPagesDbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            documents = dbContext.PageCollections;
        }

        public async Task<IPageCollection> CreateCollectionAsync(string webSiteId, string title, string pageTypeName, PageSortMode sortMode, Guid? pageId = null)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            if (pageTypeName == null)
                throw new ArgumentNullException(nameof(pageTypeName));

            var collection = new PageCollectionDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                WebsiteId = webSiteId,
                Version = 1,
                Title = title,
                PageTypeName = pageTypeName,
                SortMode = sortMode,
                PageId = pageId
            };

            await documents.InsertOneAsync(collection);

            return collection;
        }

        public async Task<IPageCollection> FindCollectiondByIdAsync(Guid id)
        {
            var filter = Builders<PageCollectionDocument>.Filter.Eq(it => it.Id, id);
            var cursor = await documents.FindAsync(filter);
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<IPageCollection>> ListCollectionsAsync(string webSiteId, Guid? pageId = null)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));

            if (pageId.HasValue)
                return await (await documents.FindAsync(it => it.WebsiteId == webSiteId && it.PageId == pageId)).ToListAsync();
            else
                return await (await documents.FindAsync(it => it.WebsiteId == webSiteId && it.PageId == null)).ToListAsync();
        }

        public async Task<IEnumerable<IPageCollection>> FindCollectionsAsync(string webSiteId, string[] pageTypeNames, string title = null)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));
            if (pageTypeNames == null)
                throw new ArgumentNullException(nameof(pageTypeNames));
            if (!pageTypeNames.Any())
                throw new ArgumentException("Require not empty.", nameof(pageTypeNames));

            var filters = new List<FilterDefinition<PageCollectionDocument>> { Builders<PageCollectionDocument>.Filter.Eq(it => it.WebsiteId, webSiteId) };

            if (pageTypeNames != null && pageTypeNames.Any())
                filters.Add(Builders<PageCollectionDocument>.Filter.Or(pageTypeNames.Select(it => Builders<PageCollectionDocument>.Filter.Eq(d => d.PageTypeName, it))));

            if (!string.IsNullOrWhiteSpace(title))
                filters.Add(Builders<PageCollectionDocument>.Filter.Text(title, new TextSearchOptions { CaseSensitive = false }));

            var filterDefinition = Builders<PageCollectionDocument>.Filter.And(filters);

            var cursor = await documents.FindAsync(filterDefinition);
            return await cursor.ToListAsync();
        }

        public async Task UpdateCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default)
        {
            var collectionDocument = (PageCollectionDocument)collection;
            var curVersion = collectionDocument.Version;
            collectionDocument.Version++;

            var updateResult = await documents.ReplaceOneAsync(it => it.Id == collection.Id && it.Version == curVersion, collectionDocument, cancellationToken: cancellationToken);
            if (updateResult.MatchedCount != 1)
                throw new InvalidOperationException();
        }

        public async Task DeleteCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default)
        {
            var collectionDocument = (PageCollectionDocument)collection;
            var curVersion = collectionDocument.Version;
            collectionDocument.Version++;

            var deleteResult = await documents.DeleteOneAsync(it => it.Id == collection.Id && it.Version == curVersion, cancellationToken: cancellationToken);
            if (deleteResult.DeletedCount != 1)
                throw new InvalidOperationException();
        }
    }
}