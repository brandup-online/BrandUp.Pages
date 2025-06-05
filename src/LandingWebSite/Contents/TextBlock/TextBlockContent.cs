using System.ComponentModel.DataAnnotations;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.TextBlock
{
    [ContentType(Title = "Текст")]
    public abstract class TextBlockContent : PageBlockContent
    {
        [Html(Placeholder = "Введите текст"), Required]
        public string Text { get; set; }
    }
}