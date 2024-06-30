using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-html")]
    public class HtmlTagHelper : FieldTagHelper<IHtmlField>
    {
        [HtmlAttributeName("content-html")]
        public override ModelExpression FieldName { get; set; }

        protected override async Task RenderContentAsync(TagHelperOutput output)
        {
            var value = Field.GetModelValue(Content) as string;
            if (Field.HasValue(value))
            {
                output.Content.SetHtmlContent(value);
            }

            output.TagMode = TagMode.StartTagAndEndTag;

            await Task.CompletedTask;
        }
    }
}