using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageCollectionRepository : IPageCollectionRepositiry
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
                filters.Add(Builders<PageCollectionDocument>.Filter.Text(title, new TextSearchOptions { CaseSensitive = false, Language = "ru" }));

            var filterDefinition = Builders<PageCollectionDocument>.Filter.And(filters);

            var cursor = await documents
                .Find(filterDefinition)
                .ToCursorAsync();

            return cursor.ToEnumerable();
        }

        public async Task<IPageCollection> UpdateCollectionAsync(Guid id, string title, PageSortMode pageSort)
        {
            var filter = Builders<PageCollectionDocument>.Filter.Eq(it => it.Id, id);
            var cursor = await documents.FindAsync(filter);
            var collection = await cursor.SingleOrDefaultAsync();

            collection.Title = title;
            collection.SortMode = pageSort;

            var updateDefinition = Builders<PageCollectionDocument>.Update
                .Set(it => it.Title, title)
                .Set(it => it.SortMode, pageSort);

            var updateResult = await documents.UpdateOneAsync(filter, updateDefinition);
            if (updateResult.MatchedCount != 1)
                throw new InvalidOperationException();

            return collection;
        }

        public async Task DeleteCollectionAsync(Guid id)
        {
            var deleteResult = await documents.DeleteOneAsync(it => it.Id == id);
            if (deleteResult.DeletedCount != 1)
                throw new InvalidOperationException();
        }
    }
}