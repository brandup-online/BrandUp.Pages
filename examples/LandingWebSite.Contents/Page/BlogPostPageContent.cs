using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.Page
{
    [PageContent(Title = "Blog post page")]
    public class BlogPostPageContent : PageContent
    {
        [Model]
        public List<PageBlockContent> Blocks { get; set; }
    }
}