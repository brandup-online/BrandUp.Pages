using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.Page
{
    [PageContent]
    public abstract class PageContent
    {
        [Text(Placeholder = "Input page title"), Title]
        public string Title { get; set; }
    }
}