using BrandUp.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace LandingWebSite.Pages
{
    public class AbountModel : AppPageModel
    {
        public override string Title => "About";
        public override string Description => "About page description";
        public override string Keywords => "about, company";
        public override string ScriptName => "about";
        public override string CssClass => "about-page";
        public override string CanonicalLink => Url.Page("", null, null, Request.Scheme, Request.Host.Value, null);

        protected override Task OnInitializeAsync(PageHandlerExecutingContext context)
        {
            SetOpenGraph("test", Title, Description);

            return base.OnInitializeAsync(context);
        }
    }
}