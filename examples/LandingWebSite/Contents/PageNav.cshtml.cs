using System.ComponentModel.DataAnnotations;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents
{
    [ContentType]
    public class PageNavContent
    {
        [Text(Title = "Текст в логотипе"), Required]
        public string LogoText { get; set; }
        [Model(Title = "Меню")]
        public List<PageNavMenuItemContent> Items { get; set; }
    }
}