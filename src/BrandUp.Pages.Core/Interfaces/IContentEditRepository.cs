namespace BrandUp.Pages.Interfaces
{
    public interface IContentEditRepository
    {
        Task<IContentEdit> CreateEditAsync(IPage page, string userId, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByUserAsync(IPage page, string userId, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDictionary<string, object>> GetContentAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default);
        Task SetContentAsync(IContentEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task DeleteEditAsync(IContentEdit pageEdit, CancellationToken cancellationToken = default);
    }
}