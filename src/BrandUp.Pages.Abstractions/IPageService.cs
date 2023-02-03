using BrandUp.Pages.Metadata;

namespace BrandUp.Pages
{
    public interface IPageService
    {
        Task<IPage> CreatePageByItemAsync<TItem, TContent>(string webSiteId, TItem item, TContent pageContent, CancellationToken cancellationToken = default)
            where TItem : class
            where TContent : class, new();
        Task<IPage> FindPageByItemAsync<TItem>(string webSiteId, TItem item, CancellationToken cancellationToken = default)
            where TItem : class;

        Task<IPage> CreatePageAsync(IPageCollection collection, object pageContent, CancellationToken cancellationToken = default);
        Task<IPage> CreatePageAsync(IPageCollection collection, string pageType = null, string pageHeader = null, CancellationToken cancellationToken = default);
        Task<IPage> FindPageByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IPage> FindPageByPathAsync(string webSiteId, string pagePath, CancellationToken cancellationToken = default);
        Task<PageUrlResult> FindUrlByPathAsync(string webSiteId, string path, CancellationToken cancellationToken = default);
        Task<IPage> GetDefaultPageAsync(string webSiteId, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> GetPublishedPagesAsync(string webSiteId, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> SearchPagesAsync(string webSiteId, string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default);
        Task<PageMetadataProvider> GetPageTypeAsync(IPage page, CancellationToken cancellationToken = default);
        Task<object> GetPageContentAsync(IPage page, CancellationToken cancellationToken = default);
        Task SetPageContentAsync(IPage page, object contentModel, CancellationToken cancellationToken = default);
        Task<Result> PublishPageAsync(IPage page, string urlPath, CancellationToken cancellationToken = default);
        Task<Result> DeletePageAsync(IPage page, CancellationToken cancellationToken = default);
        Task<Guid?> GetParentPageIdAsync(IPage page, CancellationToken cancellationToken = default);
        Task<PageSeoOptions> GetPageSeoOptionsAsync(IPage page, CancellationToken cancellationToken = default);
        Task UpdatePageSeoOptionsAsync(IPage page, PageSeoOptions seoOptions, CancellationToken cancellationToken = default);
        Task UpPagePositionAsync(IPage page, IPage beforePage, CancellationToken cancellationToken = default);
        Task DownPagePositionAsync(IPage page, IPage afterPage, CancellationToken cancellationToken = default);
    }

    public interface IPage
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        string WebsiteId { get; }
        string TypeName { get; }
        Guid OwnCollectionId { get; }
        string ItemId { get; }
        string ItemType { get; }
        string Header { get; set; }
        string UrlPath { get; }
        bool IsPublished { get; }
    }

    public enum PageSortMode
    {
        FirstOld = 0,
        FirstNew = 1
    }

    public class PageSeoOptions
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Keywords { get; set; }
    }

    public class GetPagesOptions
    {
        public Guid CollectionId { get; set; }
        public PageSortMode? SortDirection { get; set; }
        public bool? CustomSorting { get; set; }
        public bool IncludeDrafts { get; set; }
        public PagePaginationOptions Pagination { get; set; }

        public GetPagesOptions(Guid collectionId)
        {
            CollectionId = collectionId;
        }
    }

    public class PageUrlResult
    {
        public Guid? PageId { get; }
        public PageUrlRedirect Redirect { get; }

        public PageUrlResult(Guid pageId)
        {
            PageId = pageId;
        }
        public PageUrlResult(PageUrlRedirect redirect)
        {
            Redirect = redirect;
        }
    }

    public class PageUrlRedirect
    {
        public string Path { get; }
        public bool IsPermament { get; }

        public PageUrlRedirect(string path, bool isPermament)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            IsPermament = isPermament;
        }
    }

    public class PagePaginationOptions
    {
        public int Skip { get; }
        public int Limit { get; }

        public PagePaginationOptions(int skip, int limit)
        {
            if (skip < 0)
                throw new ArgumentOutOfRangeException(nameof(skip));
            if (limit <= skip || limit - skip > 50)
                throw new ArgumentOutOfRangeException(nameof(limit));

            Skip = skip;
            Limit = limit;
        }
        public PagePaginationOptions(int limit) : this(0, limit) { }
    }
}