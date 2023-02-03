using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

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
        [HtmlAttributeName("content-render-mode")]
        public FieldRenderMode RenderMode { get; set; }
        public bool IsDesigner => ContentContext.IsDesigner;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData[RazorViewRenderService.ViewData_ContentContextKeyName] is not ContentContext contentContext)
                throw new InvalidOperationException();
            ContentContext = contentContext;

            if (!contentContext.Explorer.Metadata.TryGetField(FieldName.Name, out IFieldProvider field) || field is not TField textField)
                throw new InvalidOperationException();
            Field = textField;

            var accessProvider = contentContext.Services.GetRequiredService<Identity.IAccessProvider>();
            var isAdmin = await accessProvider.CheckAccessAsync();
            if (isAdmin)
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

            if (!contentContext.IsDesigner && RenderMode == FieldRenderMode.HasValue)
            {
                var value = Field.GetModelValue(contentContext.Content);
                if (!Field.HasValue(value))
                {
                    output.SuppressOutput();
                    return;
                }
            }

            await RenderContentAsync(output);
        }

        protected abstract Task RenderContentAsync(TagHelperOutput output);
    }

    public enum FieldRenderMode
    {
        Always,
        HasValue
    }
}