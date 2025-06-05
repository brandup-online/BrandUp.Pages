using LandingWebSite.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LandingWebSite.Controllers
{
	public class AuthController : Controller
	{
		readonly UserManager<Identity.IdentityUser> userManager;
		readonly RoleManager<LandingWebSite.Identity.IdentityRole> roleManager;
		readonly SignInManager<Identity.IdentityUser> signInManager;

		public AuthController(UserManager<Identity.IdentityUser> userManager, RoleManager<LandingWebSite.Identity.IdentityRole> roleManager, SignInManager<Identity.IdentityUser> signInManager)
		{
			this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
			this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
		}

		[HttpGet("signin")]
		public async Task<IActionResult> SignInAsync()
		{
			var user = await userManager.FindByEmailAsync("test@test.ru");
			if (user == null)
			{
				var createUserResult = await userManager.CreateAsync(new Identity.IdentityUser { UserName = "test@test.ru", Email = "test@test.ru", EmailConfirmed = true });
				if (!createUserResult.Succeeded)
					return BadRequest();

				user = await userManager.FindByNameAsync("test@test.ru");

				var role = await roleManager.FindByNameAsync(RoleBasedAccessProvider.RoleName);
				if (role == null)
				{
					var createRoleResult = await roleManager.CreateAsync(new LandingWebSite.Identity.IdentityRole { Name = RoleBasedAccessProvider.RoleName });
					if (!createRoleResult.Succeeded)
						throw new InvalidOperationException();
				}

				var result = await userManager.AddToRoleAsync(user, RoleBasedAccessProvider.RoleName);
				if (!result.Succeeded)
					throw new InvalidOperationException();
			}

			await signInManager.SignInAsync(user, false);

			return Ok();
		}

		[HttpGet("signout")]
		public async Task<IActionResult> SignOutAsync()
		{
			await signInManager.SignOutAsync();

			return Ok();
		}
	}
}