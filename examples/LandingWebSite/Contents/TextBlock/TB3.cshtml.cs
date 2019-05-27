using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.TextBlock
{
    [ContentType(Title = "Текст с заголовком и фоном")]
    public class TB3 : TextBlockContent
    {
        [Text]
        public string Header { get; set; }
        [Image]
        public ImageValue Background { get; set; }
    }
}