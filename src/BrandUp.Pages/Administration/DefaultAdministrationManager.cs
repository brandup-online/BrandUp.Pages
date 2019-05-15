using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Administration
{
    public class DefaultAdministrationManager : IAdministrationManager
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public DefaultAdministrationManager(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new Exception(nameof(httpContextAccessor));
        }

        public Task<bool> CheckAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(httpContextAccessor.HttpContext.User.Identity.IsAuthenticated);
        }

        public async Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            if (!await CheckAsync(cancellationToken))
                throw new InvalidOperationException();

            var claimId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId == null)
                return null;

            return claimId.Value;
        }
    }
}