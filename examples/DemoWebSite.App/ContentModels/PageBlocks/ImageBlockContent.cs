using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace DemoWebSite.ContentModels.PageBlocks
{
    [ContentModel(Title = "Картинка")]
    public class ImageBlockContent : PageBlockContent
    {
        [Image("Изображение", IsRequired = true)]
        public ImageValue Image { get; set; }
        [Text("Название")]
        public string Title { get; set; }
        [Text("Текст вместо картинки")]
        public string Alt { get; set; }
    }
}