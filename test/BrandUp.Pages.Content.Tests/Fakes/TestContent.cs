using System.ComponentModel.DataAnnotations;
using BrandUp.Pages.Content.Fields;

namespace BrandUp.Pages.Content.Fakes
{
    [ContentType]
    public class TestContent
    {
        [Text(Placeholder = "placeholder", AllowMultiline = true), Required]
        public string Text { get; set; }

        [Html(Placeholder = "placeholder")]
        public string Html { get; set; }

        [Image]
        public ImageValue Image { get; set; }
    }
}