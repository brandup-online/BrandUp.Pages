using Microsoft.AspNetCore.Http;
using System;
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
    }
}