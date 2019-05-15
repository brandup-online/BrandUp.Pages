using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-text")]
    public class TextTagHelper : FieldTagHelper<ITextField>
    {
        [HtmlAttributeName("content-text")]
        public override ModelExpression FieldName { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            var value = Field.GetModelValue(Content) as string;

            output.Content.SetContent(value ?? string.Empty);

            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}