using BrandUp.Pages;
using BrandUp.Website.Pages;
using LandingWebSite.Contents.Page;
using LandingWebSite.Models;

namespace LandingWebSite.Pages.Blog
{
    public class BlogPostModel : PageSetModel<BlogPostDocument, BlogPostPageContent>
    {
        public override string Title => "About";

        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            return base.OnPageRequestAsync(context);
        }
    }
}