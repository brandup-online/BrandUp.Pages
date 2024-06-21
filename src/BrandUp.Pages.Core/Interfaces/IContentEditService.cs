namespace BrandUp.Pages.Interfaces
{
    public interface IContentEditService
    {
        Task<IContentEdit> BeginEditAsync(string websiteId, string contentKey, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IContentEdit> FindEditByUserAsync(string websiteId, string contentKey, CancellationToken cancellationToken = default);
        Task<object> GetContentAsync(IContentEdit editSession, CancellationToken cancellationToken = default);
        Task SetContentAsync(IContentEdit editSession, object content, CancellationToken cancellationToken = default);
        Task DiscardEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default);
        Task CommitEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default);
    }

    public interface IContentEdit
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        string WebsiteId { get; }
        string ContentKey { get; }
        string UserId { get; }
    }
}