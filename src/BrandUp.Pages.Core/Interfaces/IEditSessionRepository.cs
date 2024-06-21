namespace BrandUp.Pages.Interfaces
{
    public interface IEditSessionRepository
    {
        Task<IEditSession> CreateEditAsync(IPage page, string userId, CancellationToken cancellationToken = default);
        Task<IEditSession> FindEditByUserAsync(IPage page, string userId, CancellationToken cancellationToken = default);
        Task<IEditSession> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDictionary<string, object>> GetContentAsync(IEditSession pageEdit, CancellationToken cancellationToken = default);
        Task SetContentAsync(IEditSession pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task DeleteEditAsync(IEditSession pageEdit, CancellationToken cancellationToken = default);
    }
}