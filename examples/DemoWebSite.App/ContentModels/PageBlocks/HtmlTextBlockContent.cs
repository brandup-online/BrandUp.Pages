using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace DemoWebSite.ContentModels.PageBlocks
{
    [ContentModel(Title = "Форматированный текст")]
    public class HtmlTextBlockContent : PageBlockContent
    {
        [Html("Текст", IsRequired = true)]
        public string Html { get; set; }
    }
}