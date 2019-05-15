using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "asp-page-path", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PageLinkTagHelper : TagHelper
    {
        private readonly IPageLinkGenerator pageLinkGenerator;

        [HtmlAttributeName("asp-page-path")]
        public string PagePath { get; set; }

        public PageLinkTagHelper(IPageLinkGenerator pageLinkGenerator)
        {
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var pagePath = await pageLinkGenerator.GetUrlAsync(PagePath);

            output.Attributes.SetAttribute("href", pagePath);
        }
    }
}