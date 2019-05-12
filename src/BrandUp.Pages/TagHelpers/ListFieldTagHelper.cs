using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-list")]
    public class ListFieldTagHelper : TagHelper
    {
        [HtmlAttributeName("content-list")]
        public ModelExpression FieldName { get; set; }

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var contentContext = ViewContext.ViewData["_ContentContext_"] as ContentContext;
            if (!contentContext.Explorer.Metadata.TryGetField(FieldName.Name, out IFieldProvider field) || !(field is IListField))
                throw new Exception();

            var listField = (IListField)field;

            output.Attributes.Add("content-path", contentContext.Explorer.Path);
            output.Attributes.Add("content-field", listField.Name);
            output.TagMode = TagMode.StartTagAndEndTag;

            if (listField.GetModelValue(contentContext.Content) is IList value && value.Count > 0)
            {
                foreach (var item in value)
                {

                }
            }
        }
    }
}