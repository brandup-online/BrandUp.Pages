using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.TextBlock
{
    [Content]
    public class TextBlockContent : PageBlockContent
    {
        [Html]
        public string Text { get; set; }
    }
}