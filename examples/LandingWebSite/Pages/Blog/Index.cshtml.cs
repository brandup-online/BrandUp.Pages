using BrandUp.Website.Pages;
using LandingWebSite.Blog;
using LandingWebSite.Blog.Documents;

namespace LandingWebSite.Pages.Blog
{
    public class IndexModel : AppPageModel
    {
        readonly BlogService blogService;

        public List<BlogPostDocument> Posts { get; } = new List<BlogPostDocument>();

        public IndexModel(BlogService blogService)
        {
            this.blogService = blogService ?? throw new ArgumentNullException(nameof(blogService));
        }

        public override string Title => "Blog";
        public override string Description => "Blog page description";
        public override string Keywords => "blog, company";

        protected override Task OnPageRenderAsync(PageRenderContext context)
        {
            Posts.AddRange(blogService.Posts.OrderByDescending(it => it.CreatedDate));

            return Task.CompletedTask;
        }
    }
}