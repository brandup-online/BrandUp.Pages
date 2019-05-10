using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents
{
    [PageContent]
    public class PageContent
    {
        [Text(Placeholder = "Input page header"), PageTitle]
        public string Header { get; set; }

        [Html]
        public string Text { get; set; }

        [Image]
        public ImageValue Image { get; set; }
    }
}