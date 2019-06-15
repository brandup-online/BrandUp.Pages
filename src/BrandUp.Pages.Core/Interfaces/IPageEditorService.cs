using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageEditorService
    {
        IQueryable<IPageEditor> ContentEditors { get; }
        Task<Result<IPageEditor>> AssignEditorAsync(string email, CancellationToken cancellationToken = default);
        Task<IPageEditor> FindByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IPageEditor> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(IPageEditor pageEditor, CancellationToken cancellationToken = default);
    }

    public interface IPageEditor
    {
        string Id { get; }
        DateTime CreatedDate { get; }
        string Email { get; }
    }
}