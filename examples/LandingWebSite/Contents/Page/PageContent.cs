using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.Page
{
    [PageContent]
    public abstract class PageContent
    {
        [Text(Placeholder = "Input page title"), PageTitle]
        public string Title { get; set; }

        [Text(Placeholder = "Input page description")]
        public string Description { get; set; }

        [Text(Placeholder = "Input page keywords")]
        public string Keywords { get; set; }
    }
}