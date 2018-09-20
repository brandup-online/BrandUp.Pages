using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;

namespace BrandUp.Pages.Mvc.TagHelpers
{
    [HtmlTargetElement(Attributes = "content")]
    public class ContentFieldTagHelper : TagHelper
    {
        [HtmlAttributeName("content")]
        public ModelExpression Field { get; set; }

        [HtmlAttributeName("format")]
        public string Format { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!ViewContext.ViewData.TryGetValue("ModelContext", out object contentContextObject))
                throw new InvalidOperationException("Представление не содержит контекста контента.");
            var contentContext = (MvcContentContext)contentContextObject;

            var fieldName = Field.Name;
            if (!contentContext.Explorer.Metadata.TryGetField(fieldName, out Content.Fields.Field field))
                throw new InvalidOperationException($"Тип контента {contentContext.Explorer.Metadata.Name} не содержит поле {fieldName}.");

            var fieldValue = Field.Model;
            //if (field.HasValue(fieldValue))
            {
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Attributes.SetAttribute("data-content-path", contentContext.Explorer.Path);
                output.Attributes.SetAttribute("data-content-field", fieldName);
                output.Attributes.SetAttribute("data-content-field-designer", field.GetType().Name);

                if (field is Content.Fields.ContentListField)
                {
                    for (var i = 0; i < ((IList)fieldValue).Count; i++)
                    {
                        var itemContentExplorer = contentContext.Explorer.Navigate(fieldName + "[" + i + "]");
                        var itemContentContext = new MvcContentContext(itemContentExplorer);

                        output.Content.AppendHtml(await itemContentContext.RenderAsync(ViewContext));
                    }
                }
                else
                {
                    var valueAsString = FormatValue(fieldValue, Format);
                    output.Content.SetHtmlContent(valueAsString);
                }
            }
            //else
            //    output.TagName = null;
        }

        public static string FormatValue(object value, string format)
        {
            if (value == null)
                return string.Empty;

            if (string.IsNullOrEmpty(format))
                return Convert.ToString(value, CultureInfo.CurrentCulture);

            return string.Format(CultureInfo.CurrentCulture, format, value);
        }
    }
}