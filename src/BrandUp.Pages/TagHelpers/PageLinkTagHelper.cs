using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "asp-page-path", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PageLinkTagHelper : TagHelper
    {
        private readonly IPageLinkGenerator pageLinkGenerator;
        private readonly HtmlEncoder htmlEncoder;

        [HtmlAttributeName("asp-page-path")]
        public string PagePath { get; set; }

        public PageLinkTagHelper(IPageLinkGenerator pageLinkGenerator, HtmlEncoder htmlEncoder)
        {
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
            this.htmlEncoder = htmlEncoder ?? throw new ArgumentNullException(nameof(htmlEncoder));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var pagePath = await pageLinkGenerator.GetPathAsync(PagePath);

            output.Attributes.SetAttribute("href", pagePath);
            output.AddClass("applink", htmlEncoder);
        }
    }
}