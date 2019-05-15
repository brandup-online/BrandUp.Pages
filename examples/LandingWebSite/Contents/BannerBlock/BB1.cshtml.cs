using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace LandingWebSite.Contents.BannerBlock
{
    [ContentType(Title = "Слайдер баннеров")]
    public class BB1 : BannerBlockContent
    {
        [Content]
        public List<BB1_Item> Banners { get; set; }
    }
}