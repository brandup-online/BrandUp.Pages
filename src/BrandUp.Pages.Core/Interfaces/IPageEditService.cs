using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageEditService
    {
        Task<IPageEdit> BeginEditAsync(IPage page, CancellationToken cancellationToken = default);
        Task<IPageEdit> FindEditSessionById(Guid id, CancellationToken cancellationToken = default);
        Task<IPageEdit> FindEditByUserAsync(IPage page, CancellationToken cancellationToken = default);
        Task<object> GetContentAsync(IPageEdit editSession, CancellationToken cancellationToken = default);
        Task SetContentAsync(IPageEdit editSession, object content, CancellationToken cancellationToken = default);
        Task DiscardEditSession(IPageEdit editSession, CancellationToken cancellationToken = default);
        Task CommitEditSessionAsync(IPageEdit editSession, CancellationToken cancellationToken = default);
    }

    public interface IPageEdit
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        Guid PageId { get; }
        string UserId { get; }
    }

    public interface IPageEditRepository
    {
        Task<IPageEdit> CreateEditAsync(IPage page, string userId, CancellationToken cancellationToken = default);
        Task<IPageEdit> FindEditByUserAsync(IPage page, string userId, CancellationToken cancellationToken = default);
        Task<IPageEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDictionary<string, object>> GetContentAsync(Guid sessionId, CancellationToken cancellationToken = default);
        Task SetContentAsync(IPageEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task DeleteEditAsync(IPageEdit pageEdit, CancellationToken cancellationToken = default);
    }
}