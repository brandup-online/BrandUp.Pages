using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace LandingWebSite.Contents.BannerBlock
{
    [ContentType(Title = "Слайдер баннеров")]
    public class BB1 : BannerBlockContent
    {
        [Model]
        public List<BB1_ItemBase> Banners { get; set; }
    }

    [ContentType]
    public abstract class BB1_ItemBase : BannerContent
    {
        [Text(IsRequired = true), Title]
        public string Header { get; set; }

        [Text]
        public string SubHeader { get; set; }

        [HyperLink]
        public HyperLinkValue Link { get; set; }
    }
}