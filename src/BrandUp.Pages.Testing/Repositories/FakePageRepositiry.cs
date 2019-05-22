using BrandUp.Pages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Repositories
{
    public class FakePageRepositiry : IPageRepository
    {
        private readonly FakePageHierarhyRepository pageHierarhy;

        private int pageIndex = 0;
        private readonly Dictionary<int, Page> pages = new Dictionary<int, Page>();
        private readonly Dictionary<Guid, int> pageIds = new Dictionary<Guid, int>();
        private readonly Dictionary<string, int> pagePaths = new Dictionary<string, int>();
        private readonly Dictionary<int, IDictionary<string, object>> pageContents = new Dictionary<int, IDictionary<string, object>>();

        public FakePageRepositiry(FakePageHierarhyRepository pageHierarhy)
        {
            this.pageHierarhy = pageHierarhy ?? throw new ArgumentNullException(nameof(pageHierarhy));
        }

        public Task<IPage> CreatePageAsync(Guid ownCollectionId, string typeName, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            var pageId = Guid.NewGuid();
            var page = new Page(pageId, typeName, ownCollectionId) { Header = title, UrlPath = pageId.ToString() };

            pageIndex++;
            var index = pageIndex;

            pageIds.Add(page.Id, index);
            pageContents.Add(index, contentData);
            pagePaths.Add(page.UrlPath.ToLower(), index);
            pages.Add(pageIndex, page);

            pageHierarhy.OnAddPage(page);

            return Task.FromResult<IPage>(page);
        }
        public Task<IPage> FindPageByPathAsync(string path, CancellationToken cancellationToken = default)
        {
            if (!pagePaths.TryGetValue(path.ToLower(), out int index))
                return Task.FromResult<IPage>(null);

            var page = pages[index];

            return Task.FromResult<IPage>(page);
        }
        public Task<IPage> FindPageByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!pageIds.TryGetValue(id, out int index))
                return Task.FromResult<IPage>(null);

            var page = pages[index];

            return Task.FromResult<IPage>(page);
        }
        public Task<PageUrlResult> FindPageUrlAsync(string path, CancellationToken cancellationToken = default)
        {
            if (!pagePaths.TryGetValue(path.ToLower(), out int index))
                return Task.FromResult<PageUrlResult>(null);

            var page = pages[index];

            return Task.FromResult(new PageUrlResult(page.Id));
        }
        public Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default)
        {
            var pages = pageHierarhy.OnGetPages(options.CollectionId);
            return Task.FromResult(pages);
        }
        public Task<IEnumerable<IPage>> SearchPagesAsync(string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default)
        {
            var result = pages.Values.AsQueryable().Where(it => it.Header.Contains(title));

            if (pagination != null)
            {
                result = result.Skip(pagination.Skip);
                result = result.Take(pagination.Limit);
            }

            return Task.FromResult<IEnumerable<IPage>>(result.OfType<IPage>().ToArray());
        }
        public Task<IDictionary<string, object>> GetContentAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            if (!pageIds.TryGetValue(pageId, out int index))
                throw new InvalidOperationException();

            if (!pageContents.TryGetValue(index, out IDictionary<string, object> contentData))
                throw new InvalidOperationException();

            return Task.FromResult(contentData);
        }
        public Task SetContentAsync(Guid pageId, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default)
        {
            if (!pageIds.TryGetValue(pageId, out int index))
                throw new InvalidOperationException();
            var page = pages[index];

            page.Header = title;
            pageContents[index] = contentData;

            return Task.CompletedTask;
        }
        public Task DeletePageAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (!pageIds.TryGetValue(page.Id, out int index))
                throw new InvalidOperationException();

            pageHierarhy.OnRemovePage(page);

            pages.Remove(index);
            pageIds.Remove(page.Id);
            pagePaths.Remove(page.UrlPath.ToLower());
            pageContents.Remove(index);

            return Task.CompletedTask;
        }
        public Task<bool> HasPagesAsync(Guid ownCollectionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(pageHierarhy.HasPages(ownCollectionId));
        }

        public Task UpdatePageAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (!pageIds.TryGetValue(page.Id, out int index))
                throw new InvalidOperationException();

            var oldPage = pages[index];

            pages[index] = (Page)page;

            pagePaths.Remove(oldPage.UrlPath.ToLower());
            pagePaths.Add(page.UrlPath.ToLower(), index);

            return Task.CompletedTask;
        }

        private class Page : IPage
        {
            public Guid Id { get; }
            public DateTime CreatedDate { get; set; }
            public string TypeName { get; }
            public Guid OwnCollectionId { get; }
            public string UrlPath { get; set; }
            public string Header { get; set; }
            public int ContentVersion { get; set; } = 1;
            public bool IsPublished => Status == PageStatus.Published;
            private PageStatus Status { get; set; }

            public Page(Guid id, string typeName, Guid collectionId)
            {
                Id = id;
                CreatedDate = DateTime.UtcNow;
                TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
                OwnCollectionId = collectionId;
                Status = PageStatus.Draft;
            }

            public Task SetUrlAsync(string urlPath)
            {
                Status = PageStatus.Published;
                UrlPath = urlPath;

                return Task.CompletedTask;
            }
        }

        enum PageStatus
        {
            Draft,
            Published
        }
    }
}