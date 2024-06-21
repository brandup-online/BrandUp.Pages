namespace BrandUp.Pages.Interfaces
{
    public interface IContentRepository
    {
        Task<IDictionary<string, object>> GetContentAsync(string websiteId, string key, CancellationToken cancellationToken = default);
        Task SetContentAsync(string websiteId, string key, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
    }
}