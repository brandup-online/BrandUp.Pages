using System.ComponentModel.DataAnnotations;
using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.TextBlock
{
    [ContentType(Title = "Текст с заголовком и фоном")]
    public class TB3 : TextBlockContent
    {
        [Text, Required]
        public string Header { get; set; }
        [Image, Required]
        public ImageValue Background { get; set; }
    }
}