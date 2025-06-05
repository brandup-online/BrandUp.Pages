using BrandUp.Pages.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
    public class PagesTagHelperComponent(IAccessProvider accessProvider) : TagHelperComponent
    {
        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase) && await accessProvider.CheckAccessAsync(ViewContext.HttpContext.RequestAborted))
                output.Attributes.Add("data-pages-admin", "true");
        }
    }
}