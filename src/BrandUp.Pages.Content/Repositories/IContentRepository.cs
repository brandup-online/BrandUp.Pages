namespace BrandUp.Pages.Content.Repositories
{
    public interface IContentRepository
    {
        Task<IContent> FindByKeyAsync(string websiteId, string key, CancellationToken cancellationToken = default);
        Task<ContentDataResult> GetDataAsync(string websiteId, string key, CancellationToken cancellationToken = default);
        Task<ContentDataResult> SetDataAsync(string websiteId, string key, string prevVersion, string type, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
    }

    public class ContentDataResult
    {
        public string Type { get; set; }
        public IDictionary<string, object> Data { get; set; }
        public string Version { get; set; }
    }
}