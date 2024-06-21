namespace BrandUp.Pages.Interfaces
{
    public interface IEditSessionService
    {
        Task<IEditSession> BeginEditAsync(IPage page, CancellationToken cancellationToken = default);
        Task<IEditSession> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEditSession> FindEditByUserAsync(IPage page, CancellationToken cancellationToken = default);
        Task<object> GetContentAsync(IEditSession editSession, CancellationToken cancellationToken = default);
        Task SetContentAsync(IEditSession editSession, object content, CancellationToken cancellationToken = default);
        Task DiscardEditAsync(IEditSession editSession, CancellationToken cancellationToken = default);
        Task CommitEditAsync(IEditSession editSession, CancellationToken cancellationToken = default);
    }

    public interface IEditSession
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        Guid PageId { get; }
        string UserId { get; }
    }
}