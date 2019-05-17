using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.TextBlock
{
    [ContentType]
    public abstract class TextBlockContent : PageBlockContent
    {
        [Html(Placeholder = "Введите текст")]
        public string Text { get; set; }
    }
}