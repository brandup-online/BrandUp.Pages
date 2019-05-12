using BrandUp.Pages;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LandingWebSite.Pages
{
    public class SignInModel : PageModel, IContentPageModel
    {
        public string Title => "Sign in";
        public string Description => null;
        public string Keywords => null;
    }
}