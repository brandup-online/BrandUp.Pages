using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-text")]
    public class TextFieldTagHelper : TagHelper
    {
        [HtmlAttributeName("content-text")]
        public ModelExpression FieldName { get; set; }

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!(ViewContext.ViewData["_ContentContext_"] is ContentContext contentContext))
                throw new InvalidOperationException();
            if (!contentContext.Explorer.Metadata.TryGetField(FieldName.Name, out IFieldProvider field) || !(field is ITextField textField))
                throw new InvalidOperationException();

            var value = textField.GetModelValue(contentContext.Content) as string;

            output.Attributes.Add("content-path", contentContext.Explorer.Path);
            output.Attributes.Add("content-field", textField.Name);
            output.Content.SetContent(value ?? string.Empty);
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}