namespace BrandUp.Pages.Interfaces
{
    public interface IContentRepository
    {
        Task<IDictionary<string, object>> GetDataAsync(string websiteId, string key, CancellationToken cancellationToken = default);
        Task SetDataAsync(string websiteId, string key, string type, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
    }
}