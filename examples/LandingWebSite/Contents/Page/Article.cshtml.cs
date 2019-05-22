using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace LandingWebSite.Contents.Page
{
    [PageContent(Title = "Article page")]
    public class ArticlePageContent : PageContent
    {
        [Text(Placeholder = "Input page sub header")]
        public string SubHeader { get; set; }

        [Model]
        public List<PageBlockContent> Blocks { get; set; }
    }
}