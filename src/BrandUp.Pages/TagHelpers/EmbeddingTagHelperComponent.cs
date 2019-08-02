using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    public class EmbeddingTagHelperComponent : TagHelperComponent
    {
        private readonly IJsonHelper jsonHelper;

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public EmbeddingTagHelperComponent(IJsonHelper jsonHelper)
        {
            this.jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is AppPageModel appPageModel)
            {
                if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(appPageModel.CssClass))
                    {
                        string cssClass = null;
                        if (output.Attributes.TryGetAttribute("class", out TagHelperAttribute attribute))
                            cssClass = (string)attribute.Value;

                        if (!string.IsNullOrEmpty(cssClass))
                            cssClass += " " + appPageModel.CssClass;
                        else
                            cssClass = appPageModel.CssClass;

                        output.Attributes.SetAttribute("class", cssClass);
                    }
                }

                if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
                {
                    var appClientModel = await appPageModel.GetAppClientModelAsync(ViewContext.HttpContext.RequestAborted);

                    output.PostContent.AppendHtml("<script type=\"text/javascript\">var appInitOptions = " + jsonHelper.Serialize(appClientModel) + "</script>");
                }
            }
        }
    }
}