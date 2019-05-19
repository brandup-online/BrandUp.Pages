using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement("content-element", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ContentElementTagHelper : TagHelper
    {
        [HtmlAttributeName("tag")]
        public string HtmlTag { get; set; } = "div";

        [HtmlAttributeName("class")]
        public string CssClass { get; set; }

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!(ViewContext.ViewData[RazorViewRenderService.ViewData_ViewRenderingContextKeyName] is ViewRenderingContext contentRenderingContext))
                throw new InvalidOperationException();

            output.SuppressOutput();

            contentRenderingContext.HtmlTag = HtmlTag;
            contentRenderingContext.CssClass = CssClass;
        }
    }
}