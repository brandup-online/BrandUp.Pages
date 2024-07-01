using System.ComponentModel.DataAnnotations;
using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.MobileMenu
{
    [ContentType]
    public class MobileMenuContent
    {
        [Model(Title = "Меню")]
        public List<MenuItem> Items { get; set; }

        [ContentType]
        public class MenuItem
        {
            [Text, Title, Required]
            public string Text { get; set; }
            [HyperLink, Required]
            public HyperLinkValue Url { get; set; }
        }
    }
}