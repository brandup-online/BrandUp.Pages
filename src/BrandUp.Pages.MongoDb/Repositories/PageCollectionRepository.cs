using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageCollectionRepository : IPageCollectionRepository
    {
        readonly IMongoCollection<PageCollectionDocument> documents;

        public PageCollectionRepository(IPagesDbContext dbContext)
        {
            documents = dbContext.PageCollections;
        }

        public async Task<IPageCollection> CreateCollectionAsync(string title, string pageTypeName, PageSortMode sortMode, Guid? pageId)
        {
            var collection = new PageCollectionDocument
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
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

        public async Task<IEnumerable<IPageCollection>> GetCollectionsAsync(Guid? pageId)
        {
            var cursor = await documents
                .Find(it => it.PageId == pageId)
                .ToCursorAsync();
            return cursor.ToEnumerable();
        }

        public async Task<IEnumerable<IPageCollection>> GetCollectionsAsync(string[] pageTypeNames, string title)
        {
            var filters = new List<FilterDefinition<PageCollectionDocument>>
            {
                Builders<PageCollectionDocument>.Filter.Or(pageTypeNames.Select(it => Builders<PageCollectionDocument>.Filter.Eq(d => d.PageTypeName, it)))
            };

            if (!string.IsNullOrWhiteSpace(title))
                filters.Add(Builders<PageCollectionDocument>.Filter.Text(title, new TextSearchOptions { CaseSensitive = false }));

            var filterDefinition = Builders<PageCollectionDocument>.Filter.And(filters);

            var cursor = await documents
                .Find(filterDefinition)
                .ToCursorAsync();

            return cursor.ToEnumerable();
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