using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Data.Repositories
{
    public class FakePageRepositiry : IPageRepositiry
    {
        private readonly IPageMetadataManager pageMetadataManager;
        private readonly FakePageHierarhyRepository pageHierarhy;

        private int pageIndex = 0;
        private readonly Dictionary<int, Page> pages = new Dictionary<int, Page>();
        private readonly Dictionary<Guid, int> pageIds = new Dictionary<Guid, int>();
        private readonly Dictionary<string, int> pagePaths = new Dictionary<string, int>();
        private readonly Dictionary<int, PageContent> pageContents = new Dictionary<int, PageContent>();
        private Guid defaultPageId = Guid.Empty;

        public FakePageRepositiry(FakePageHierarhyRepository pageHierarhy, IPageMetadataManager pageMetadataManager)
        {
            this.pageHierarhy = pageHierarhy ?? throw new ArgumentNullException(nameof(pageHierarhy));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
        }

        public Task<IPage> CreatePageAsync(Guid ownCollectionId, string typeName, IDictionary<string, object> contentData)
        {
            var page = new Page(Guid.NewGuid(), typeName, ownCollectionId);

            pageIndex++;
            var index = pageIndex;

            pageIds.Add(page.Id, index);
            pageContents.Add(index, new PageContent(1, contentData));
            pages.Add(pageIndex, page);

            pageHierarhy.OnAddPage(page);

            return Task.FromResult<IPage>(page);
        }
        public Task<IPage> FindPageByPathAsync(string path)
        {
            if (!pagePaths.TryGetValue(path.ToLower(), out int index))
                return Task.FromResult<IPage>(null);

            var page = pages[index];

            return Task.FromResult<IPage>(page);
        }
        public Task<IPage> FindPageByIdAsync(Guid id)
        {
            if (!pageIds.TryGetValue(id, out int index))
                return Task.FromResult<IPage>(null);

            var page = pages[index];

            return Task.FromResult<IPage>(page);
        }
        public async Task<IPage> GetDefaultPageAsync()
        {
            if (defaultPageId == Guid.Empty)
                return null;

            var defualtPage = await FindPageByIdAsync(defaultPageId);
            if (defualtPage == null)
                throw new InvalidOperationException();

            return defualtPage;
        }
        public Task<IEnumerable<IPage>> GetPagesAsync(Guid ownCollectionId, PageSortMode sortMode, PagePaginationOptions pagination)
        {
            var pages = pageHierarhy.OnGetPages(ownCollectionId);
            return Task.FromResult(pages);
        }
        public Task SetDefaultPageAsync(IPage page)
        {
            defaultPageId = page.Id;

            return Task.CompletedTask;
        }
        public Task<PageContent> GetContentAsync(Guid pageId)
        {
            if (!pageIds.TryGetValue(pageId, out int index))
                throw new InvalidOperationException();

            if (!pageContents.TryGetValue(index, out PageContent content))
                throw new InvalidOperationException();

            return Task.FromResult(content);
        }
        public Task SetContentAsync(Guid pageId, PageContent data)
        {
            if (!pageIds.TryGetValue(pageId, out int index))
                throw new InvalidOperationException();
            var page = pages[index];

            if (page.ContentVersion != data.Version)
                throw new InvalidOperationException();

            page.ContentVersion = data.Version + 1;
            pageContents[index] = data;

            return Task.CompletedTask;
        }
        public Task SetUrlPathAsync(Guid pageId, string urlPath)
        {
            if (!pageIds.TryGetValue(pageId, out int index))
                throw new InvalidOperationException();

            pages[index].UrlPath = urlPath;

            pagePaths.Add(urlPath.ToLower(), index);

            return Task.CompletedTask;
        }
        public Task DeletePageAsync(Guid pageId)
        {
            if (!pageIds.TryGetValue(pageId, out int index))
                throw new InvalidOperationException();
            var page = pages[index];

            pageHierarhy.OnRemovePage(page);

            pages.Remove(index);
            pageIds.Remove(pageId);
            if (page.UrlPath != null)
                pagePaths.Remove(page.UrlPath.ToLower());
            pageContents.Remove(index);

            return Task.CompletedTask;
        }

        public Task<bool> HasPagesAsync(Guid ownCollectionId)
        {
            return Task.FromResult(pageHierarhy.HasPages(ownCollectionId));
        }

        private class Page : IPage
        {
            public Guid Id { get; }
            public DateTime CreatedDate { get; set; }
            public string TypeName { get; }
            public Guid OwnCollectionId { get; }
            public string UrlPath { get; set; }
            public int ContentVersion { get; set; } = 1;

            public Page(Guid id, string typeName, Guid collectionId)
            {
                Id = id;
                CreatedDate = DateTime.UtcNow;
                TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
                OwnCollectionId = collectionId;
            }
        }
    }
}