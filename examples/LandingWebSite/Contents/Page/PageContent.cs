using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents
{
    [PageContent]
    public class PageContent
    {
        [Text, PageTitle]
        public string Header { get; set; }
    }
}