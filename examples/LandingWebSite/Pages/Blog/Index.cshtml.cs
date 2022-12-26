using BrandUp.Pages;
using BrandUp.Website.Pages;
using System;
using System.Threading.Tasks;

namespace LandingWebSite.Pages.Blog
{
    public class IndexModel : PageSetModel<BlogPostDocument>
    {
        public override string Title => "About";

        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            return base.OnPageRequestAsync(context);
        }
    }

    public class BlogPostDocument
    {
        public Guid Id { get; set; }
    }
}