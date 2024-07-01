using BrandUp.Pages.Content;
using BrandUp.Pages.Views;
using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement("page", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PageTagHelper(ContentMetadataManager contentMetadataManager) : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public override int Order => int.MaxValue;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is not AppPageModel pageModel)
                return;

            if (ViewContext.ViewData[RazorViewRenderService.ViewData_ViewRenderingContextKeyName] is not ContentRenderingContext)
                return;

            var pageModelType = pageModel.GetType();
            foreach (var @interface in pageModelType.GetInterfaces())
            {
                if (!@interface.IsConstructedGenericType)
                    continue;

                var gType = @interface.GetGenericTypeDefinition();
                if (gType == typeof(IContentPage<>))
                {
                    var contentType = @interface.GenericTypeArguments[0];
                    var contentProvider = contentMetadataManager.GetMetadata(contentType);

                    var keyProperty = @interface.GetProperty("ContentKey", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var contentKey = (string)keyProperty.GetValue(pageModel);

                    output.Attributes.Add("data-content-root", contentKey);
                    output.Attributes.Add("data-content-type", contentProvider.Name);
                    output.Attributes.Add("data-content-title", contentProvider.Title);
                    output.Attributes.Add("data-content-path", null);

                    break;
                }
            }

            base.Process(context, output);
        }
    }
}