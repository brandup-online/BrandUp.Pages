using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Administration
{
    public interface IAdministrationManager
    {
        Task<bool> CheckAsync(CancellationToken cancellationToken = default);
        Task<string> GetUserIdAsync(CancellationToken cancellationToken = default);
    }
}