using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageEditorRepository
    {
        IQueryable<IPageEditor> ContentEditors { get; }
        Task AssignEditorAsync(string email, CancellationToken cancellationToken = default);
        Task<IPageEditor> FindByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IPageEditor> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task DeleteAsync(IPageEditor pageEditor, CancellationToken cancellationToken = default);
    }
}