using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "asp-page-path", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PageLinkTagHelper : TagHelper
    {
        private readonly HtmlEncoder htmlEncoder;

        [HtmlAttributeName("asp-page-path")]
        public string PagePath { get; set; }
        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public PageLinkTagHelper(HtmlEncoder htmlEncoder)
        {
            this.htmlEncoder = htmlEncoder ?? throw new ArgumentNullException(nameof(htmlEncoder));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var pageLinkGenerator = ViewContext.HttpContext.RequestServices.GetService<IPageLinkGenerator>();
            if (pageLinkGenerator == null)
                return;

            var pagePath = await pageLinkGenerator.GetPathAsync(PagePath);

            output.Attributes.SetAttribute("href", pagePath);
            output.AddClass("applink", htmlEncoder);
        }
    }
}