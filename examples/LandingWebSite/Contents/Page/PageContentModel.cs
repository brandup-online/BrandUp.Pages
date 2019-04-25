using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents
{
    [PageContentModel(Name = "Page")]
    public class PageContentModel
    {
        [Text("Header"), PageTitle]
        public string Header { get; set; }
    }
}