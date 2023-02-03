using BrandUp.Website.Pages;
using LandingWebSite.Blog;

namespace LandingWebSite.Pages.Blog
{
    public class IndexModel : AppPageModel
    {
        readonly BlogService blogService;

        public IndexModel(BlogService blogService)
        {
            this.blogService = blogService ?? throw new ArgumentNullException(nameof(blogService));
        }

        public override string Title => "Blog";
        public override string Description => "Blog page description";
        public override string Keywords => "blog, company";
    }
}