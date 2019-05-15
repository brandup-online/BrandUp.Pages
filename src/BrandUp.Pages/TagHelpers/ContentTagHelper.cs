using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-list")]
    public class ContentTagHelper : TagHelper
    {
        private readonly IJsonHelper jsonHelper;
        private readonly IHtmlHelper htmlHelper;

        [HtmlAttributeName("content-list")]
        public ModelExpression FieldName { get; set; }

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public ContentTagHelper(IJsonHelper jsonHelper, IHtmlHelper htmlHelper)
        {
            this.jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
            this.htmlHelper = htmlHelper ?? throw new ArgumentNullException(nameof(htmlHelper));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!(ViewContext.ViewData["_ContentContext_"] is ContentContext contentContext))
                throw new InvalidOperationException();
            if (!contentContext.Explorer.Metadata.TryGetField(FieldName.Name, out IFieldProvider field) || !(field is IContentField listField))
                throw new InvalidOperationException();

            (htmlHelper as IViewContextAware).Contextualize(ViewContext);

            var fieldModel = new Models.ContentFieldModel
            {
                Type = field.Type,
                Name = field.Name,
                Title = field.Title,
                Options = field.GetFormOptions(contentContext.Services)
            };

            var viewRenderService = ViewContext.HttpContext.RequestServices.GetRequiredService<IViewRenderService>();

            output.Attributes.Add("content-path", contentContext.Explorer.Path);
            output.Attributes.Add("content-field", listField.Name);
            output.Attributes.Add("content-field-type", listField.Type);
            output.Attributes.Add(new TagHelperAttribute("content-field-model", jsonHelper.Serialize(fieldModel).ToString(), HtmlAttributeValueStyle.SingleQuotes));

            output.TagMode = TagMode.StartTagAndEndTag;

            if (listField.IsListValue)
            {
                var list = listField.GetModelValue(contentContext.Content) as IList;
                if (list.Count > 0)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var itemContentContext = contentContext.Navigate($"{FieldName.Name}[{i}]");
                        var itemHtml = await viewRenderService.RenderToStringAsync(itemContentContext);

                        output.Content.AppendHtmlLine(itemHtml);
                    }
                }
            }
            else
            {
                var value = listField.GetModelValue(contentContext.Content);
                if (listField.HasValue(value))
                {
                    var itemContentContext = contentContext.Navigate(FieldName.Name);
                    var itemHtml = await viewRenderService.RenderToStringAsync(itemContentContext);

                    output.Content.AppendHtmlLine(itemHtml);
                }
            }
        }
    }

    public class ContentRenderingContext
    {
        public string HtmlTag { get; set; } = "div";
        public string CssClass { get; set; }
    }
}