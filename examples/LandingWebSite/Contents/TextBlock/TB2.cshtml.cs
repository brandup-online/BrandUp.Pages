using System.ComponentModel.DataAnnotations;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.TextBlock
{
    [ContentType(Title = "Текст с заголовком")]
    public class TB2 : TextBlockContent
    {
        [Text, Required]
        public string Header { get; set; }
    }
}