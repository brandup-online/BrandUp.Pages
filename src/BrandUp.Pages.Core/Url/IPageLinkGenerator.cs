namespace BrandUp.Pages.Url
{
    public interface IPageLinkGenerator
    {
        Task<string> GetItemPathAsync<TItem>(TItem item, CancellationToken cancellationToken = default) where TItem : class;
        Task<string> GetPathAsync(IPage page, CancellationToken cancellationToken = default);
        Task<string> GetUrlAsync(IPage page, CancellationToken cancellationToken = default);
        Task<string> GetPathAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default);
        Task<string> GetUrlAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default);
        Task<string> GetPathAsync(string pagePath, CancellationToken cancellationToken = default);
        Task<string> GetUrlAsync(string pagePath, CancellationToken cancellationToken = default);
    }
}