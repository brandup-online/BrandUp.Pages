using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    public class ListFieldTagHelper : TagHelper
    {
        private readonly IJsonHelper jsonHelper;
        private readonly IHtmlHelper htmlHelper;

        [HtmlAttributeName("content-list")]
        public ModelExpression FieldName { get; set; }

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public ListFieldTagHelper(IJsonHelper jsonHelper, IHtmlHelper htmlHelper)
        {
            this.jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
            this.htmlHelper = htmlHelper ?? throw new ArgumentNullException(nameof(htmlHelper));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!(ViewContext.ViewData["_ContentContext_"] is ContentContext contentContext))
                throw new InvalidOperationException();
            if (!contentContext.Explorer.Metadata.TryGetField(FieldName.Name, out IFieldProvider field) || !(field is IListField listField))
                throw new InvalidOperationException();

            (htmlHelper as IViewContextAware).Contextualize(ViewContext);

            var fieldModel = new Models.ContentFieldModel
            {
                Type = field.Type,
                Name = field.Name,
                Title = field.Title,
                Options = field.GetFormOptions(contentContext.Services)
            };

            var viewLocator = ViewContext.HttpContext.RequestServices.GetRequiredService<Views.IViewLocator>();

            output.Attributes.Add("content-path", contentContext.Explorer.Path);
            output.Attributes.Add("content-field", listField.Name);
            output.Attributes.Add("content-field-type", listField.Type);
            output.Attributes.Add(new TagHelperAttribute("content-field-model", jsonHelper.Serialize(fieldModel).ToString(), HtmlAttributeValueStyle.SingleQuotes));

            output.TagMode = TagMode.StartTagAndEndTag;

            if (listField.GetModelValue(contentContext.Content) is IList value && value.Count > 0)
            {
                for (var i = 0; i < value.Count; i++)
                {
                    var itemContentContext = contentContext.Navigate(FieldName.Name + "[" + i + "]");

                    var itemView = viewLocator.FindView(itemContentContext.Explorer.Metadata.ModelType);
                    if (itemView == null)
                        throw new Exception();

                    var itemViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = itemContentContext.Content
                    };
                    itemViewData.Add("_ContentContext_", itemContentContext);

                    var itemRenderingContext = new ContentRenderingContext();
                    itemViewData.Add("_ContentRenderingContext_", itemRenderingContext);

                    var itemHtml = await htmlHelper.PartialAsync("~" + itemView.Name, itemContentContext.Content, itemViewData);

                    string tagName = "div";

                    if (!string.IsNullOrEmpty(itemRenderingContext.HtmlTag))
                        tagName = itemRenderingContext.HtmlTag;

                    var tag = new TagBuilder(tagName);
                    if (!string.IsNullOrEmpty(itemRenderingContext.CssClass))
                        tag.AddCssClass(itemRenderingContext.CssClass);

                    tag.Attributes.Add("content-path", itemContentContext.Explorer.Path);
                    tag.Attributes.Add("content-path-index", i.ToString());

                    tag.InnerHtml.AppendHtml(itemHtml);

                    output.Content.AppendLine(tag);

                    i++;
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