using BrandUp.Pages;
using BrandUp.Website.Pages;
using LandingWebSite.Blog.Documents;
using LandingWebSite.Contents.Page;

namespace LandingWebSite.Pages.Blog
{
    public class BlogPostModel : ItemContentPageModel<BlogPostDocument, BlogPostPageContent>
    {
        public override string Title => "About";

        protected override Task OnPageRequestAsync(PageRequestContext context)
        {
            return base.OnPageRequestAsync(context);
        }
    }
}