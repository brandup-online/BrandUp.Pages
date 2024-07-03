using BrandUp.Pages.Content.Items;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Services;

namespace BrandUp.Pages.Repositories
{
    public class FakePageRepositiry(FakePageHierarhyRepository pageHierarhy) : IPageRepository
    {
        int pageIndex = 0;
        readonly Dictionary<int, Page> pages = [];
        readonly Dictionary<Guid, int> pageIds = [];
        readonly Dictionary<string, int> pagePaths = [];

        public async Task<IPage> CreatePageAsync(string websiteId, Guid сollectionId, Guid pageId, string typeName, string pageHeader, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            if (pageId == Guid.Empty)
                pageId = Guid.NewGuid();

            var page = new Page(pageId, websiteId, typeName, сollectionId) { Header = pageHeader, UrlPath = pageId.ToString() };

            pageIndex++;
            var index = pageIndex;

            pageIds.Add(page.Id, index);
            pagePaths.Add(page.WebsiteId.ToLower() + ":" + page.UrlPath.ToLower(), index);
            pages.Add(pageIndex, page);

            pageHierarhy.OnAddPage(page);

            return page;
        }

        public Task<IPage> FindPageByPathAsync(string webSiteId, string path, CancellationToken cancellationToken = default)
        {
            if (!pagePaths.TryGetValue(webSiteId.ToLower() + ":" + path.ToLower(), out int index))
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

        public Task<PageUrlResult> FindUrlByPathAsync(string webSiteId, string path, CancellationToken cancellationToken = default)
        {
            if (!pagePaths.TryGetValue(webSiteId.ToLower() + ":" + path.ToLower(), out int index))
                return Task.FromResult<PageUrlResult>(null);

            var page = pages[index];

            return Task.FromResult(new PageUrlResult(page.Id));
        }

        public Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default)
        {
            var pages = pageHierarhy.OnGetPages(options.CollectionId);
            return Task.FromResult(pages);
        }

        public Task<IEnumerable<IPage>> GetPublishedPagesAsync(string websiteId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<IPage>>(pages.Values.Where(it => it.WebsiteId == websiteId && it.IsPublished));
        }

        public Task<IEnumerable<IPage>> SearchPagesAsync(string websiteId, string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default)
        {
            var result = pages.Values.AsQueryable().Where(it => it.WebsiteId == websiteId && it.Header.Contains(title));

            if (pagination != null)
            {
                result = result.Skip(pagination.Skip);
                result = result.Take(pagination.Limit);
            }

            return Task.FromResult<IEnumerable<IPage>>([.. result.OfType<IPage>()]);
        }

        public async Task DeletePageAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (!pageIds.TryGetValue(page.Id, out int index))
                throw new InvalidOperationException();

            pageHierarhy.OnRemovePage(page);

            pages.Remove(index);
            pageIds.Remove(page.Id);
            pagePaths.Remove(page.UrlPath.ToLower());

            await Task.CompletedTask;
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

            pagePaths.Remove(page.WebsiteId.ToLower() + ":" + oldPage.UrlPath.ToLower());
            pagePaths.Add(page.WebsiteId.ToLower() + ":" + page.UrlPath.ToLower(), index);

            return Task.CompletedTask;
        }

        public async Task SetPageHeaderAsync(IPage page, string header, CancellationToken cancellationToken = default)
        {
            var pageDocument = (Page)page;

            pageDocument.Header = header;

            await Task.CompletedTask;
        }

        public Task SetUrlPathAsync(IPage page, string urlPath, CancellationToken cancellationToken = default)
        {
            if (urlPath == null)
                throw new ArgumentNullException(nameof(urlPath));

            var pageDocument = (Page)page;

            pageDocument.UrlPath = urlPath;
            pageDocument.Status = PageStatus.Published;

            return Task.CompletedTask;
        }

        public Task<string> GetPageTitleAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var p = (Page)page;
            return Task.FromResult(p.SeoTitle);
        }
        public Task SetPageTitleAsync(IPage page, string title, CancellationToken cancellationToken = default)
        {
            var p = (Page)page;
            p.SeoTitle = title;
            return Task.CompletedTask;
        }
        public Task<string> GetPageDescriptionAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var p = (Page)page;
            return Task.FromResult(p.SeoDescription);
        }
        public Task SetPageDescriptionAsync(IPage page, string description, CancellationToken cancellationToken = default)
        {
            var p = (Page)page;
            p.SeoDescription = description;
            return Task.CompletedTask;
        }
        public Task<string[]> GetPageKeywordsAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var p = (Page)page;
            return Task.FromResult(p.SeoKeywords);
        }
        public Task SetPageKeywordsAsync(IPage page, string[] keywords, CancellationToken cancellationToken = default)
        {
            var p = (Page)page;
            p.SeoKeywords = keywords;
            return Task.CompletedTask;
        }
        public Task UpPagePositionAsync(IPage page, IPage beforePage, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public Task DownPagePositionAsync(IPage page, IPage afterPage, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        class Page : IPage
        {
            public Guid Id { get; }
            public DateTime CreatedDate { get; set; }
            public string WebsiteId { get; set; }
            public string TypeName { get; }
            public Guid OwnCollectionId { get; }
            public string UrlPath { get; set; }
            public string Header { get; set; }
            public int ContentVersion { get; set; } = 1;
            public bool IsPublished => Status == PageStatus.Published;
            public PageStatus Status { get; set; }
            public string SeoTitle { get; set; }
            public string SeoDescription { get; set; }
            public string[] SeoKeywords { get; set; }

            string IItemContent.ItemId => Id.ToString();

            public Page(Guid id, string webSiteId, string typeName, Guid collectionId)
            {
                Id = id;
                CreatedDate = DateTime.UtcNow;
                WebsiteId = webSiteId;
                TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
                OwnCollectionId = collectionId;
                Status = PageStatus.Draft;
            }
        }

        enum PageStatus
        {
            Draft,
            Published
        }
    }
}