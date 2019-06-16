using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Identity
{
    public interface IAccessProvider
    {
        Task<string> GetUserIdAsync(CancellationToken cancellationToken = default);
        Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default);
    }
}