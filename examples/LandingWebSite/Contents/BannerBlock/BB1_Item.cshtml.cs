using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.BannerBlock
{
    [ContentType]
    public class BB1_Item : BannerContent
    {
        [Text(IsRequired = true), Title]
        public string Header { get; set; }

        [Text]
        public string SubHeader { get; set; }
    }
}