namespace BrandUp.Pages.Interfaces
{
    public interface IContentEditService
    {
        Task<IContentEdit> BeginEditAsync(IPage page, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByUserAsync(IPage page, CancellationToken cancellationToken = default);
        Task<object> GetContentAsync(IContentEdit editSession, CancellationToken cancellationToken = default);
        Task SetContentAsync(IContentEdit editSession, object content, CancellationToken cancellationToken = default);
        Task DiscardEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default);
        Task CommitEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default);
    }

    public interface IContentEdit
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        Guid PageId { get; }
        string UserId { get; }
    }
}