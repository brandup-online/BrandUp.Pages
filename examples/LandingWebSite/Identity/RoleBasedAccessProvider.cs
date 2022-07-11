using BrandUp.Pages.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LandingWebSite.Identity
{
    public class RoleBasedAccessProvider : IAccessProvider
    {
        public const string RoleName = "BrandUpPages.Editor";
        private readonly IHttpContextAccessor httpContextAccessor;

        public RoleBasedAccessProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new Exception(nameof(httpContextAccessor));
        }

        public Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            var claimId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId == null)
                return null;

            return Task.FromResult(claimId.Value);
        }

        public Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default)
        {
            if (!httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                return Task.FromResult(false);

            return Task.FromResult(httpContextAccessor.HttpContext.User.IsInRole(RoleName));
        }
    }
}