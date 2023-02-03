namespace BrandUp.Pages.Repositories
{
    public interface IPageRepository
    {
        Task<IPage> CreatePageAsync(string websiteId, Guid сollectionId, string typeName, string pageHeader, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task<IPage> FindPageByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IPage> FindPageByPathAsync(string websiteId, string path, CancellationToken cancellationToken = default);
        Task<PageUrlResult> FindUrlByPathAsync(string websiteId, string path, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> GetPublishedPagesAsync(string websiteId, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> SearchPagesAsync(string webSiteId, string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default);
        Task<bool> HasPagesAsync(Guid сollectionId, CancellationToken cancellationToken = default);
        Task<IDictionary<string, object>> GetContentAsync(Guid pageId, CancellationToken cancellationToken = default);
        Task SetContentAsync(Guid pageId, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task UpdatePageAsync(IPage page, CancellationToken cancellationToken = default);
        Task DeletePageAsync(IPage page, CancellationToken cancellationToken = default);
        Task SetUrlPathAsync(IPage page, string urlPath, CancellationToken cancellationToken = default);
        Task<string> GetPageTitleAsync(IPage page, CancellationToken cancellationToken = default);
        Task SetPageTitleAsync(IPage page, string title, CancellationToken cancellationToken = default);
        Task<string> GetPageDescriptionAsync(IPage page, CancellationToken cancellationToken = default);
        Task SetPageDescriptionAsync(IPage page, string description, CancellationToken cancellationToken = default);
        Task<string[]> GetPageKeywordsAsync(IPage page, CancellationToken cancellationToken = default);
        Task SetPageKeywordsAsync(IPage page, string[] keywords, CancellationToken cancellationToken = default);
        Task UpPagePositionAsync(IPage page, IPage beforePage, CancellationToken cancellationToken = default);
        Task DownPagePositionAsync(IPage page, IPage afterPage, CancellationToken cancellationToken = default);
    }
}