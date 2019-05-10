using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-html")]
    public class HtmlFieldTagHelper : TagHelper
    {
        [HtmlAttributeName("content-html")]
        public ModelExpression FieldName { get; set; }

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var contentContext = ViewContext.ViewData["_ContentContext_"] as ContentContext;
            if (!contentContext.Explorer.Metadata.TryGetField(FieldName.Name, out IFieldProvider field) || !(field is HtmlAttribute))
                throw new Exception();

            var htmlField = (HtmlAttribute)field;
            var value = htmlField.GetModelValue(contentContext.Content) as string;

            output.Attributes.Add("content-path", contentContext.Explorer.Path);
            output.Attributes.Add("content-field", htmlField.Name);
            output.Content.SetContent(value ?? string.Empty);
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}