using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace DemoWebSite.ContentModels.PageBlocks
{
    [ContentModel(Title = "Блок страницы")]
    public abstract class PageBlockContent
    {
        [View]
        public string ViewName { get; set; }
    }
}