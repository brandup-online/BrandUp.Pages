using BrandUp.Pages.Content;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Mvc.TagHelpers
{
    [HtmlTargetElement(Attributes = "bind-image", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ImageFieldTagHelper : TagHelper
    {
        [HtmlAttributeName("bind-image")]
        public ModelExpression ContentValue { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!ViewContext.ViewData.TryGetValue("ModelContext", out object contentContextObject))
                throw new InvalidOperationException("Представление не содержит контекста контента.");
            var contentContext = (MvcContentContext)contentContextObject;

            var fieldName = ContentValue.Name;
            if (!contentContext.Explorer.Metadata.TryGetField(fieldName, out Content.Fields.ImageField field))
                throw new InvalidOperationException($"Тип контента {contentContext.Explorer.Metadata.Name} не содержит поле {fieldName}.");

            var fieldValue = ContentValue.Model;

            output.Attributes.SetAttribute("data-content-path", contentContext.Explorer.Path);
            output.Attributes.SetAttribute("data-content-field", fieldName);
            output.Attributes.SetAttribute("data-content-field-designer", field.GetType().Name);

            if (field.HasValue(fieldValue))
            {
                var imageValue = (Content.Fields.ImageValue)fieldValue;
                var imageUrl = "_file/" + imageValue.FileId;

                if (output.TagName == "img")
                {
                    output.Attributes.SetAttribute("src", imageUrl);
                    output.TagMode = TagMode.SelfClosing;
                }
                else
                {
                    output.TagMode = TagMode.StartTagAndEndTag;

                    if (!output.Attributes.TryGetAttribute("style", out TagHelperAttribute styleAttribute))
                        output.Attributes.Add(styleAttribute = new TagHelperAttribute("style"));

                    var styleValue = styleAttribute.Value as string;
                    if (string.IsNullOrWhiteSpace(styleValue))
                        styleValue = string.Empty;
                    else
                        styleValue += ";";

                    styleValue += "background-image: url(" + imageUrl + ");";

                    output.Attributes.SetAttribute("style", styleValue);
                }
            }

            return Task.CompletedTask;
        }
    }
}