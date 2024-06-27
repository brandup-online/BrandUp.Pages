namespace BrandUp.Pages.Content.Repositories
{
    public interface IContentEditRepository
    {
        Task<IContentEdit> CreateEditAsync(string websiteId, string contentKey, string contentVersion, string userId, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByUserAsync(string websiteId, string contentKey, string userId, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDictionary<string, object>> GetContentAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default);
        Task SetContentAsync(IContentEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task DeleteEditAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default);
    }
}