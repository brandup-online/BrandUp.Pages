using System.Collections;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-object")]
    public class ContentTagHelper(IViewRenderService viewRenderService) : FieldTagHelper<IModelField>
    {
        [HtmlAttributeName("content-object")]
        public override ModelExpression FieldName { get; set; }

        protected override async Task RenderContentAsync(TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;

            if (Field.IsListValue)
            {
                var list = Field.GetModelValue(Content) as IList;
                if (Field.HasValue(list))
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var itemContentContext = ContentContext.Navigate($"{FieldName.Name}[{i}]");
                        var itemHtml = await viewRenderService.RenderToStringAsync(itemContentContext);

                        output.Content.AppendHtmlLine(itemHtml);
                    }
                }
            }
            else
            {
                var value = Field.GetModelValue(Content);
                if (Field.HasValue(value))
                {
                    var itemContentContext = ContentContext.Navigate(FieldName.Name);
                    var itemHtml = await viewRenderService.RenderToStringAsync(itemContentContext);

                    output.Content.AppendHtmlLine(itemHtml);
                }
            }
        }
    }
}