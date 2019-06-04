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
            if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
            {
                if (ViewContext.ViewData.Model is AppPageModel appPageModel)
                {
                    var appClientModel = await appPageModel.GetAppClientModelAsync(ViewContext.HttpContext.RequestAborted);

                    output.PreContent.AppendHtml("<script type=\"text/javascript\">var appInitOptions = " + jsonHelper.Serialize(appClientModel) + "</script>");
                }
            }
        }
    }
}