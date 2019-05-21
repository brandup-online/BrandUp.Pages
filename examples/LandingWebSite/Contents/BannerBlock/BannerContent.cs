using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.BannerBlock
{
    [ContentType]
    public abstract class BannerContent
    {
        [Image]
        public ImageValue Image { get; set; }
    }
}