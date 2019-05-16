using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-link")]
    public class HyperLinkTagHelper : FieldTagHelper<IHyperLinkField>
    {
        private readonly IPageService pageService;
        private readonly IPageLinkGenerator pageLinkGenerator;

        [HtmlAttributeName("content-link")]
        public override ModelExpression FieldName { get; set; }

        public HyperLinkTagHelper(IPageLinkGenerator pageLinkGenerator, IPageService pageService)
        {
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            var value = Field.GetModelValue(Content);
            if (Field.HasValue(value))
            {
                var hyperLinkValue = (HyperLinkValue)value;

                string url;
                switch (hyperLinkValue.ValueType)
                {
                    case HyperLinkType.Url:
                        url = hyperLinkValue.Value;
                        break;
                    case HyperLinkType.Page:
                        var page = await pageService.FindPageByIdAsync(Guid.Parse(hyperLinkValue.Value));
                        if (page == null)
                            return;
                        url = await pageLinkGenerator.GetUrlAsync(page);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                output.Attributes.Add("href", url);
            }
        }
    }
}