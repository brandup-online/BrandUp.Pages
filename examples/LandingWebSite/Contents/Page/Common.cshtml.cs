using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace LandingWebSite.Contents.Page
{
    [PageContent(Title = "Common page")]
    public class CommonPageContent : PageContent
    {
        [Model]
        public List<PageBlockContent> Blocks { get; set; }
    }
}