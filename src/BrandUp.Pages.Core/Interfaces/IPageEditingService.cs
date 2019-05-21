using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageEditingService
    {
        Task<IPageEditSession> BeginEditAsync(IPage page, CancellationToken cancellationToken = default);
        Task<IPageEditSession> FindEditSessionById(Guid id, CancellationToken cancellationToken = default);
        Task<object> GetContentAsync(IPageEditSession editSession, CancellationToken cancellationToken = default);
        Task SetContentAsync(IPageEditSession editSession, object content, CancellationToken cancellationToken = default);
        Task DiscardEditSession(IPageEditSession editSession, CancellationToken cancellationToken = default);
        Task CommitEditSessionAsync(IPageEditSession editSession, CancellationToken cancellationToken = default);
    }

    public interface IPageEditSession
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        Guid PageId { get; }
        string ContentManagerId { get; }
    }

    public interface IPageEditSessionRepository
    {
        Task<IPageEditSession> CreateEditSessionAsync(Guid pageId, string contentManagerId, PageContent content);
        Task<IPageEditSession> FindEditSessionByIdAsync(Guid id);
        Task<PageContent> GetContentAsync(Guid sessionId);
        Task SetContentAsync(Guid sessionId, PageContent content);
        Task DeleteEditSessionAsync(Guid sessionId);
    }
}