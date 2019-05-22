using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Administration
{
    public class FakeAdministrationManager : IAdministrationManager
    {
        public Task<bool> CheckAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult("test");
        }
    }
}