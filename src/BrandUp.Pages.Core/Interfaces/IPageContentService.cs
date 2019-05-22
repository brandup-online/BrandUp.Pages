using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageContentService
    {
        Task<IPageEdit> BeginEditAsync(IPage page, CancellationToken cancellationToken = default);
        Task<IPageEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IPageEdit> FindEditByUserAsync(IPage page, CancellationToken cancellationToken = default);
        Task<object> GetContentAsync(IPageEdit editSession, CancellationToken cancellationToken = default);
        Task SetContentAsync(IPageEdit editSession, object content, CancellationToken cancellationToken = default);
        Task DiscardEditAsync(IPageEdit editSession, CancellationToken cancellationToken = default);
        Task CommitEditAsync(IPageEdit editSession, CancellationToken cancellationToken = default);
    }

    public interface IPageEdit
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        Guid PageId { get; }
        string UserId { get; }
    }
}