using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LandingWebSite.Pages
{
    public abstract class AppPageModel : PageModel
    {
        public abstract string Title { get; }
        public virtual string Description { get; }
        public virtual string Keywords { get; }
    }
}