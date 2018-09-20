using BrandUp.Pages.Content;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace BrandUp.Pages.Mvc.TagHelpers
{
    [HtmlTargetElement(Attributes = "bind-list", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ContentListFieldTagHelper : TagHelper
    {
        private MvcJsonOptions jsonOptions;

        [HtmlAttributeName("bind-list")]
        public ModelExpression ContentValue { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public ContentListFieldTagHelper(IOptions<MvcJsonOptions> options)
        {
            jsonOptions = options.Value;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!ViewContext.ViewData.TryGetValue("ModelContext", out object contentContextObject))
                throw new InvalidOperationException("Представление не содержит контекста контента.");
            var contentContext = (MvcContentContext)contentContextObject;

            var fieldName = ContentValue.Name;
            if (!contentContext.Explorer.Metadata.TryGetField(fieldName, out Content.Fields.ContentListField field))
                throw new InvalidOperationException($"Тип контента {contentContext.Explorer.Metadata.Name} не содержит поле {fieldName}.");

            var fieldValue = ContentValue.Model;

            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("data-content-path", contentContext.Explorer.Path);
            output.Attributes.SetAttribute("data-content-field", fieldName);
            output.Attributes.SetAttribute("data-content-field-designer", field.GetType().Name);

            var formFieldOptions = field.GetFormOptions();
            output.Attributes.SetAttribute("data-content-field-options", JsonConvert.SerializeObject(formFieldOptions, jsonOptions.SerializerSettings));

            if (field.HasValue(fieldValue))
            {
                for (var i = 0; i < ((IList)fieldValue).Count; i++)
                {
                    var itemContentExplorer = contentContext.Explorer.Navigate(fieldName + "[" + i + "]");
                    var itemContentContext = new MvcContentContext(itemContentExplorer);

                    output.Content.AppendHtml(await itemContentContext.RenderAsync(ViewContext));
                }
            }
        }
    }
}