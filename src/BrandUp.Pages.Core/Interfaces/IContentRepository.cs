namespace BrandUp.Pages.Interfaces
{
    public interface IContentRepository
    {
        Task<IDictionary<string, object>> GetContentAsync(string key, CancellationToken cancellationToken = default);
        Task SetContentAsync(string key, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
    }
}