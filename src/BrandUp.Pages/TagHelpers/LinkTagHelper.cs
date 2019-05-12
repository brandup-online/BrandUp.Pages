using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "content-page")]
    public class LinkTagHelper : TagHelper
    {
        private readonly IPageLinkGenerator pageLinkGenerator;

        [HtmlAttributeName("content-page")]
        public string PagePath { get; set; }

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public LinkTagHelper(IPageLinkGenerator pageLinkGenerator)
        {
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var url = await pageLinkGenerator.GetUrlAsync(PagePath);

            output.Attributes.Add("href", url);
        }
    }
}