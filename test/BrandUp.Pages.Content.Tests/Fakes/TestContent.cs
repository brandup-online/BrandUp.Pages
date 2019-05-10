using BrandUp.Pages.Content.Fields;

namespace BrandUp.Pages.Content.Fakes
{
    [Content]
    public class TestContent
    {
        [Text(Placeholder = "placeholder", AllowMultiline = true)]
        public string Text { get; set; }

        [Html(Placeholder = "placeholder")]
        public string Html { get; set; }

        [Image]
        public ImageValue Image { get; set; }
    }
}