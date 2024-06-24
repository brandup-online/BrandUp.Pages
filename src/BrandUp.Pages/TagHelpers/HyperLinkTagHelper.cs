using System.Text.Encodings.Web;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-link")]
    public class HyperLinkTagHelper(IPageLinkGenerator pageLinkGenerator, IPageService pageService, HtmlEncoder htmlEncoder) : FieldTagHelper<IHyperLinkField>
    {
        [HtmlAttributeName("content-link")]
        public override ModelExpression FieldName { get; set; }

        protected override async Task RenderContentAsync(TagHelperOutput output)
        {
            var value = Field.GetModelValue(Content);
            if (!Field.HasValue(value))
                return;

            var hyperLink = (HyperLinkValue)value;

            string url;
            string path = null;
            switch (hyperLink.ValueType)
            {
                case HyperLinkType.Url:
                    var uri = (Uri)hyperLink;
                    url = uri.OriginalString;
                    if (!uri.IsAbsoluteUri)
                    {
                        path = uri.OriginalString;
                        output.AddClass("applink", htmlEncoder);
                    }
                    break;
                case HyperLinkType.Page:
                    if (!Guid.TryParse(hyperLink.Value, out var pageId))
                        throw new InvalidOperationException($"Unable to parse page ID from {typeof(HyperLinkValue).Name} value.");
                    var page = await pageService.FindPageByIdAsync(pageId, ViewContext.HttpContext.RequestAborted);
                    if (page == null)
                        return;
                    if (!page.IsPublished)
                        return;
                    url = path = await pageLinkGenerator.GetPathAsync(page);

                    output.AddClass("applink", htmlEncoder);

                    break;
                default:
                    throw new InvalidOperationException($"Unknown {typeof(HyperLinkType).Name} value.");
            }

            if (path != null)
            {
                string requestPath;
                if (ViewContext.HttpContext.Request.Path.HasValue)
                    requestPath = ViewContext.HttpContext.Request.Path.Value;
                else
                    requestPath = string.Empty;

                if (requestPath.StartsWith(path))
                    output.AddClass("selected", htmlEncoder);
            }

            output.Attributes.Add("href", url);
        }
    }
}