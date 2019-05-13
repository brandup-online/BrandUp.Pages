using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
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
                    var appClientModel = await GetAppClientModelAsync(appPageModel);

                    output.PreContent.AppendHtml("<script type=\"text/javascript\">var appInitOptions = " + jsonHelper.Serialize(appClientModel) + "</script>");
                }
            }
        }

        private async Task<Models.AppClientModel> GetAppClientModelAsync(AppPageModel appPageModel)
        {
            var httpContext = ViewContext.HttpContext;
            var httpRequest = httpContext.Request;

            var appClientModel = new Models.AppClientModel
            {
                BaseUrl = httpRequest.PathBase.HasValue ? httpRequest.PathBase.Value : "/"
            };

            var antiforgery = httpContext.RequestServices.GetService<IAntiforgery>();
            if (antiforgery != null)
            {
                var antiforgeryToken = antiforgery.GetAndStoreTokens(httpContext);

                appClientModel.Antiforgery = new Models.AntiforgeryModel
                {
                    HeaderName = antiforgeryToken.HeaderName,
                    FormFieldName = antiforgeryToken.FormFieldName
                };
            }

            appClientModel.Nav = await appPageModel.GetNavigationModelAsync();

            return appClientModel;
        }
    }
}