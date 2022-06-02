using BrandUp.Pages.Interfaces;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Repositories
{
    public class FakePageHierarhyRepository
    {
        private readonly Dictionary<string, IList<IPageCollection>> collectionsByWebSites = new Dictionary<string, IList<IPageCollection>>();
        private readonly Dictionary<Guid, IList<IPageCollection>> collectionsByPages = new Dictionary<Guid, IList<IPageCollection>>();
        private readonly Dictionary<Guid, IList<IPage>> pagesByCollections = new Dictionary<Guid, IList<IPage>>();

        public FakePageHierarhyRepository()
        {
            collectionsByPages.Add(Guid.Empty, new List<IPageCollection>());
        }

        public void OnAddCollection(Guid? pageId, IPageCollection pageCollection)
        {
            if (pageId.HasValue)
            {
                if (!collectionsByPages.TryGetValue(pageId.Value, out IList<IPageCollection> pageCollectionsByPages))
                    throw new InvalidOperationException();

                pageCollectionsByPages.Add(pageCollection);
            }
            else
            {
                if (!collectionsByWebSites.TryGetValue(pageCollection.WebSiteId, out IList<IPageCollection> pageCollectionsByWebsites))
                    collectionsByWebSites.Add(pageCollection.WebSiteId, pageCollectionsByWebsites = new List<IPageCollection>());

                pageCollectionsByWebsites.Add(pageCollection);
            }

            pagesByCollections.Add(pageCollection.Id, new List<IPage>());
        }
        public IEnumerable<IPageCollection> OnGetCollections(string webSiteId, Guid? pageId)
        {
            IList<IPageCollection> pageCollections;

            if (pageId.HasValue)
            {
                if (!collectionsByPages.TryGetValue(pageId.Value, out pageCollections))
                    throw new InvalidOperationException();
            }
            else
            {
                if (!collectionsByWebSites.TryGetValue(webSiteId, out pageCollections))
                    throw new InvalidOperationException();
            }

            return pageCollections;
        }
        public void OnRemoveCollection(IPageCollection pageCollection)
        {
            var pages = pagesByCollections[pageCollection.Id];
            if (pages.Count > 0)
                throw new InvalidOperationException("Нельзя удалить коллекцию страниц, так как она содержит страницы.");

            collectionsByPages[pageCollection.PageId ?? Guid.Empty].Remove(pageCollection);
            pagesByCollections.Remove(pageCollection.Id);
        }

        public void OnAddPage(IPage page)
        {
            if (!pagesByCollections.TryGetValue(page.OwnCollectionId, out IList<IPage> pagesByCollection))
                throw new InvalidOperationException();
            pagesByCollection.Add(page);

            collectionsByPages.Add(page.Id, new List<IPageCollection>());
        }
        public IEnumerable<IPage> OnGetPages(Guid ownCollectionId)
        {
            if (!pagesByCollections.TryGetValue(ownCollectionId, out IList<IPage> pages))
                throw new ArgumentException();
            return pages;
        }
        public void OnRemovePage(IPage page)
        {
            if (!pagesByCollections.TryGetValue(page.OwnCollectionId, out IList<IPage> pages))
                throw new InvalidOperationException();
            if (!pages.Remove(page))
                throw new InvalidOperationException();

            collectionsByPages.Remove(page.Id);
        }
        public bool HasPages(Guid ownCollectionId)
        {
            if (!pagesByCollections.TryGetValue(ownCollectionId, out IList<IPage> pages))
                throw new ArgumentException();
            return pages.Count > 0;
        }
    }
}