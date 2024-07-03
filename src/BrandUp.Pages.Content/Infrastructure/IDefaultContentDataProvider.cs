namespace BrandUp.Pages.Content.Infrastructure
{
    public interface IDefaultContentDataProvider
    {
        Task<IDictionary<string, object>> GetDefaultAsync(ContentMetadata contentMetadata, CancellationToken cancellationToken);
    }
}