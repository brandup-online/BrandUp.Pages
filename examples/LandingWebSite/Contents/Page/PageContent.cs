using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.Page
{
    [PageContent]
    public abstract class PageContent
    {
        [Text(Placeholder = "Input page title"), PageTitle]
        public string Title { get; set; }
    }
}