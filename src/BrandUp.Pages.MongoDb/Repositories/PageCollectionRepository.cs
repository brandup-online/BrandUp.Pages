using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Data.Documents;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Data.Repositories
{
    public class PageCollectionRepository : MongoRepository<PageCollectionDocument>, IPageCollectionRepositiry
    {
        public PageCollectionRepository(WebSiteContext dbContext) : base(dbContext, dbContext.GetPageCollectionDocuments()) { }

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

            await AddAsync(collection);

            return collection;
        }

        public async Task<IPageCollection> FindCollectiondByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<IEnumerable<IPageCollection>> GetCollectionsAsync(Guid? pageId)
        {
            var cursor = await mongoCollection
                .Find(it => it.PageId == pageId)
                .ToCursorAsync();
            return cursor.ToEnumerable();
        }

        public async Task<IPageCollection> UpdateCollectionAsync(Guid id, string title, PageSortMode pageSort)
        {
            var collection = await GetByIdAsync(id);

            collection.Title = title;
            collection.SortMode = pageSort;

            var updateDefinition = Builders<PageCollectionDocument>.Update
                .Set(it => it.Title, title)
                .Set(it => it.SortMode, pageSort);
            var updateResult = await mongoCollection.UpdateOneAsync(it => it.Id == id, updateDefinition);

            if (updateResult.MatchedCount != 1)
                throw new InvalidOperationException();

            return collection;
        }

        public async Task DeleteCollectionAsync(Guid id)
        {
            var pages = dbContext.GetPageDocuments();

            using (var session = await dbContext.Client.StartSessionAsync())
            {
                //session.StartTransaction();

                try
                {
                    var countCollections = await pages.CountDocumentsAsync(session, it => it.OwnCollectionId == id);
                    if (countCollections > 0)
                        throw new InvalidOperationException("Удаляемая коллекция не должна содержать страницы.");

                    await mongoCollection.FindOneAndDeleteAsync(session, it => it.Id == id);

                    //await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    //await session.AbortTransactionAsync();

                    throw ex;
                }
            }
        }
    }
}