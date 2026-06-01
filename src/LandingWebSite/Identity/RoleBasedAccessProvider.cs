using System.Security.Claims;
using BrandUp.Pages.Identity;

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

		public Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
		{
			var claimId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
			if (claimId == null)
				return Task.FromResult<string?>(null);

			return Task.FromResult<string?>(claimId.Value);
		}

		public Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default)
		{
			var user = httpContextAccessor.HttpContext?.User;
			if (user?.Identity == null || !user.Identity.IsAuthenticated)
				return Task.FromResult(false);

			return Task.FromResult(user.IsInRole(RoleName));
		}
	}
}