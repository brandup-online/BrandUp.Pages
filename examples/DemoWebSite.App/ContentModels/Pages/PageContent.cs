using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace DemoWebSite.ContentModels.Pages
{
    [PageModel(Title = "Страница")]
    public class PageContent
    {
        [View]
        public string ViewName { get; set; }

        [PageTitle, Text("Название")]
        public string Title { get; set; }

        [ContentList("Контент страницы")]
        public List<PageBlocks.PageBlockContent> Blocks { get; set; }
    }
}