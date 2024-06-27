namespace BrandUp.Pages.Content.Infrastructure
{
    public interface IDefaultContentDataProvider
    {
        Task<IDictionary<string, object>> GetDefaultAsync(ContentMetadataProvider contentMetadata, CancellationToken cancellationToken);
    }
}