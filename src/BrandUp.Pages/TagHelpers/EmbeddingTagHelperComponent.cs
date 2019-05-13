using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

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

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
            {
                if (ViewContext.ViewData.Model is AppPageModel appPageModel)
                {
                    var appClientModel = GetAppClientModel(appPageModel);

                    output.PreContent.AppendHtml("<script type=\"text/javascript\">var appInitOptions = " + jsonHelper.Serialize(appClientModel) + "</script>");
                }
            }
        }

        private Models.AppClientModel GetAppClientModel(AppPageModel appPageModel)
        {
            var httpContext = ViewContext.HttpContext;
            var httpRequest = httpContext.Request;

            var appClientModel = new Models.AppClientModel
            {
                BaseUrl = httpRequest.PathBase.HasValue ? httpRequest.PathBase.Value : "/"
            };

            appClientModel.Nav = appPageModel.GetNavigationModel();

            return appClientModel;
        }
    }
}