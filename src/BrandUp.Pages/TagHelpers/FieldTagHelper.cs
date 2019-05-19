using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    public abstract class FieldTagHelper<TField> : TagHelper
        where TField : IFieldProvider
    {
        public abstract ModelExpression FieldName { get; set; }
        [HtmlAttributeName("content-designer")]
        public string DesignerName { get; set; }
        [HtmlAttributeNotBound]
        public TField Field { get; private set; }
        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }
        public ContentContext ContentContext { get; private set; }
        public object Content => ContentContext.Content;
        protected IJsonHelper JsonHelper => ViewContext.HttpContext.RequestServices.GetRequiredService<IJsonHelper>();

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!(ViewContext.ViewData[RazorViewRenderService.ViewData_ContentContextKeyName] is ContentContext contentContext))
                throw new InvalidOperationException();
            ContentContext = contentContext;

            if (!contentContext.Explorer.Metadata.TryGetField(FieldName.Name, out IFieldProvider field) || !(field is TField textField))
                throw new InvalidOperationException();
            Field = textField;

            var administrationManager = contentContext.Services.GetRequiredService<Administration.IAdministrationManager>();
            if (await administrationManager.CheckAsync())
            {
                var fieldModel = new Models.ContentFieldModel
                {
                    Type = field.Type,
                    Name = field.Name,
                    Title = field.Title,
                    Options = field.GetFormOptions(contentContext.Services)
                };

                output.Attributes.Add("content-path", contentContext.Explorer.ModelPath);
                output.Attributes.Add("content-field", textField.Name);
                output.Attributes.Add(new TagHelperAttribute("content-field-model", JsonHelper.Serialize(fieldModel).ToString(), HtmlAttributeValueStyle.SingleQuotes));

                var designerName = DesignerName;
                if (string.IsNullOrEmpty(designerName))
                    designerName = Field.Type;
                output.Attributes.Add("content-designer", designerName.ToLower());
            }

            await RenderContentAsync(output);
        }

        protected abstract Task RenderContentAsync(TagHelperOutput output);
    }
}