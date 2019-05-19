using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace LandingWebSite.Contents.Page
{
    [PageContent(Title = "Common page")]
    public class CommonPageContent : PageContent
    {
        [Text(Placeholder = "Input page header")]
        public string Header { get; set; }

        [Model]
        public List<PageBlockContent> Blocks { get; set; }
    }
}