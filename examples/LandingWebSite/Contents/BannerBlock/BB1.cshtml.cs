using System.ComponentModel.DataAnnotations;
using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.BannerBlock
{
    [ContentType(Title = "Слайдер баннеров")]
    public class BB1 : BannerBlockContent
    {
        [Model, Required]
        public List<BB1_ItemBase> Banners { get; set; }
    }

    [ContentType]
    public abstract class BB1_ItemBase
    {
        [Image, Required]
        public ImageValue Image { get; set; }

        [Text, Title, Required]
        public string Header { get; set; }

        [Text(AllowMultiline = true)]
        public string SubHeader { get; set; }

        [HyperLink, Required]
        public HyperLinkValue Link { get; set; }
    }
}