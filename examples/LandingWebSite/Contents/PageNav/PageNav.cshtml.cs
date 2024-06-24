using System.ComponentModel.DataAnnotations;
using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.PageNav
{
    [ContentType]
    public class PageNavContent
    {
        [Text(Title = "Текст в логотипе"), Required]
        public string LogoText { get; set; }
        [Model(Title = "Меню")]
        public List<MenuItem> Items { get; set; }

        [ContentType]
        public class MenuItem
        {
            [Text, Title, Required]
            public string Text { get; set; }
            [HyperLink]
            public HyperLinkValue Url { get; set; }
        }
    }
}