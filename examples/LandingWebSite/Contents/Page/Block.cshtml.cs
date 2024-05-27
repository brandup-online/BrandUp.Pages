using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.Page
{
    [PageContent(Title = "Block page")]
    public class BlockModel : PageContent
    {
        [Model]
        public List<PageBlockContent> Blocks { get; set; }
    }
}