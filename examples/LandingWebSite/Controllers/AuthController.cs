using BrandUp.Pages.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LandingWebSite.Controllers
{
    public class AuthController : Controller
    {
        readonly UserManager<Identity.IdentityUser> userManager;
        readonly SignInManager<Identity.IdentityUser> signInManager;
        readonly IUserProvider userProvider;

        public AuthController(UserManager<Identity.IdentityUser> userManager, SignInManager<Identity.IdentityUser> signInManager, IUserProvider userProvider)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
        }

        [HttpGet("signin")]
        public async Task<IActionResult> SignIn()
        {
            var user = await userManager.FindByEmailAsync("test@test.ru");
            if (user == null)
            {
                var createUserResult = await userManager.CreateAsync(new Identity.IdentityUser { UserName = "test@test.ru", Email = "test@test.ru", EmailConfirmed = true });
                if (!createUserResult.Succeeded)
                    return BadRequest();

                user = await userManager.FindByNameAsync("test@test.ru");

                var pageEditor = await userProvider.FindUserByNameAsync("test@test.ru");
                var assignResult = await userProvider.AssignUserAsync(pageEditor);
                if (!assignResult.Succeeded)
                    return BadRequest();
            }

            await signInManager.SignInAsync(user, false);

            return Ok();
        }

        [HttpGet("signout")]
        public async Task<IActionResult> SignOut()
        {
            await signInManager.SignOutAsync();

            return Ok();
        }
    }
}