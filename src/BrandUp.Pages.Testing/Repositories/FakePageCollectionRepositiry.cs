using BrandUp.Pages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrandUp.Pages.Repositories
{
    public class FakePageCollectionRepositiry : IPageCollectionRepositiry
    {
        private int collectionIndex = 0;
        private readonly Dictionary<int, PageCollection> collections = new Dictionary<int, PageCollection>();
        private readonly Dictionary<Guid, int> collectionIds = new Dictionary<Guid, int>();
        private readonly FakePageHierarhyRepository pageHierarhy;

        public FakePageCollectionRepositiry(FakePageHierarhyRepository pageHierarhy)
        {
            this.pageHierarhy = pageHierarhy ?? throw new ArgumentNullException(nameof(pageHierarhy));
        }

        public Task<IPageCollection> CreateCollectionAsync(string title, string pageTypeName, PageSortMode sortMode, Guid? pageId)
        {
            var pageCollectionId = Guid.NewGuid();
            var pageCollection = new PageCollection(pageCollectionId, title, pageTypeName, pageId)
            {
                SortMode = sortMode
            };

            collectionIndex++;
            var index = collectionIndex;

            collections.Add(index, pageCollection);
            collectionIds.Add(pageCollectionId, index);

            pageHierarhy.OnAddCollection(pageId, pageCollection);

            return Task.FromResult<IPageCollection>(pageCollection);
        }
        public Task<IPageCollection> FindCollectiondByIdAsync(Guid id)
        {
            if (!collectionIds.TryGetValue(id, out int index))
                return Task.FromResult<IPageCollection>(null);

            return Task.FromResult<IPageCollection>(collections[index]);
        }
        public Task<IEnumerable<IPageCollection>> GetCollectionsAsync(Guid? pageId)
        {
            var pageCollections = pageHierarhy.OnGetCollections(pageId);
            return Task.FromResult(pageCollections);
        }
        public Task<IPageCollection> UpdateCollectionAsync(Guid id, string title, PageSortMode pageSort)
        {
            if (!collectionIds.TryGetValue(id, out int index))
                throw new InvalidOperationException();

            var collection = collections[index];

            collection.Title = title;
            collection.SortMode = pageSort;

            return Task.FromResult<IPageCollection>(collection);
        }
        public Task DeleteCollectionAsync(Guid id)
        {
            if (!collectionIds.TryGetValue(id, out int index))
                throw new InvalidOperationException();
            var collection = collections[index];

            pageHierarhy.OnRemoveCollection(collection);

            collectionIds.Remove(id);
            collections.Remove(index);

            return Task.CompletedTask;
        }
        public Task<IEnumerable<IPageCollection>> GetCollectionsAsync(string[] pageTypeNames, string title)
        {
            var result = new List<IPageCollection>();

            foreach (var collection in collections.Values)
            {
                if (pageTypeNames.Any(it => it.ToLower() == collection.PageTypeName.ToLower()))
                {
                    if (title != null && !collection.Title.Contains(title))
                        continue;

                    result.Add(collection);
                }
            }

            return Task.FromResult<IEnumerable<IPageCollection>>(result);
        }

        private class PageCollection : IPageCollection
        {
            public Guid Id { get; }
            public DateTime CreatedDate { get; set; }
            public string Title { get; set; }
            public string PageTypeName { get; }
            public Guid? PageId { get; }
            public PageSortMode SortMode { get; set; }

            public PageCollection(Guid id, string title, string pageTypeName, Guid? pageId)
            {
                Id = id;
                CreatedDate = DateTime.UtcNow;
                Title = title;
                PageTypeName = pageTypeName;
                PageId = pageId;
            }
        }
    }
}