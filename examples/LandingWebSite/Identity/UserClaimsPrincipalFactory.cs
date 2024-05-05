using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LandingWebSite.Identity
{
	public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser>
	{
		readonly RoleManager<IdentityRole> roleManager;

		public UserClaimsPrincipalFactory(UserManager<IdentityUser> userManager, IOptions<IdentityOptions> optionsAccessor, RoleManager<IdentityRole> roleManager) : base(userManager, optionsAccessor)
		{
			this.roleManager = roleManager ?? throw new System.ArgumentNullException(nameof(roleManager));
		}

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
		{
			var identity = await base.GenerateClaimsAsync(user);

			if (user.Roles != null)
			{
				foreach (var roleName in user.Roles)
				{
					var role = await roleManager.FindByNameAsync(roleName);
					if (role != null)
						identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
				}
			}

			return identity;
		}
	}
}