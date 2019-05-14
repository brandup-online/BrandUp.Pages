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
        private readonly IJsonHelper jsonHelper;

        [HtmlAttributeName("content-html")]
        public ModelExpression FieldName { get; set; }

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public HtmlFieldTagHelper(IJsonHelper jsonHelper)
        {
            this.jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!(ViewContext.ViewData["_ContentContext_"] is ContentContext contentContext))
                throw new InvalidOperationException();
            if (!contentContext.Explorer.Metadata.TryGetField(FieldName.Name, out IFieldProvider field) || !(field is HtmlAttribute htmlField))
                throw new InvalidOperationException();

            var fieldModel = new Models.ContentFieldModel
            {
                Type = field.Type,
                Name = field.Name,
                Title = field.Title,
                Options = field.GetFormOptions(contentContext.Services)
            };

            var value = htmlField.GetModelValue(contentContext.Content) as string;

            output.Attributes.Add("content-path", contentContext.Explorer.Path);
            output.Attributes.Add("content-field", htmlField.Name);
            output.Attributes.Add("content-field", htmlField.Name);
            output.Attributes.Add("content-field-type", htmlField.Type);
            output.Attributes.Add(new TagHelperAttribute("content-field-model", jsonHelper.Serialize(fieldModel).ToString(), HtmlAttributeValueStyle.SingleQuotes));

            output.Content.SetHtmlContent(value ?? string.Empty);

            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}