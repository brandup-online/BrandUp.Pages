using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-link")]
    public class HyperLinkTagHelper : FieldTagHelper<IHyperLinkField>
    {
        private readonly IPageService pageService;
        private readonly IPageLinkGenerator pageLinkGenerator;
        private readonly HtmlEncoder htmlEncoder;

        [HtmlAttributeName("content-link")]
        public override ModelExpression FieldName { get; set; }

        public HyperLinkTagHelper(IPageLinkGenerator pageLinkGenerator, IPageService pageService, HtmlEncoder htmlEncoder)
        {
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.htmlEncoder = htmlEncoder ?? throw new ArgumentNullException(nameof(htmlEncoder));
        }

        protected override async Task RenderContentAsync(TagHelperOutput output)
        {
            var value = Field.GetModelValue(Content);
            if (Field.HasValue(value))
            {
                var hyperLinkValue = (HyperLinkValue)value;

                string url;
                switch (hyperLinkValue.ValueType)
                {
                    case HyperLinkType.Url:
                        var uri = (Uri)hyperLinkValue;
                        url = uri.OriginalString;
                        if (!uri.IsAbsoluteUri)
                            output.AddClass("applink", htmlEncoder);
                        break;
                    case HyperLinkType.Page:
                        var page = await pageService.FindPageByIdAsync(Guid.Parse(hyperLinkValue.Value));
                        if (page == null)
                            return;
                        if (!page.IsPublished)
                            return;
                        url = await pageLinkGenerator.GetUrlAsync(page);

                        output.AddClass("applink", htmlEncoder);

                        break;
                    default:
                        throw new InvalidOperationException();
                }

                output.Attributes.Add("href", url);
            }
        }
    }
}