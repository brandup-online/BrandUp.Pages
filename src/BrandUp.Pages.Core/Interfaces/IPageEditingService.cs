using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageEditingService
    {
        Task<IPageEditSession> BeginEditAsync(IPage page);
        Task<IPageEditSession> FindEditSessionById(Guid id);
        Task<object> GetContentAsync(IPageEditSession editSession);
        Task SetContentAsync(IPageEditSession editSession, object content);
        Task DiscardEditSession(IPageEditSession editSession);
        Task CommitEditSessionAsync(IPageEditSession editSession);
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
        Task DeleteEditSession(Guid sessionId);
    }
}