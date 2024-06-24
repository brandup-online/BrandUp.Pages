using System.ComponentModel.DataAnnotations;
using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents
{
    [ContentType]
    public class PageNavMenuItemContent
    {
        [Text, Title, Required]
        public string Text { get; set; }
        [HyperLink]
        public HyperLinkValue Url { get; set; }
    }
}