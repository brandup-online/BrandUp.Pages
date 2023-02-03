using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Files;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-image")]
    public class ImageTagHelper : FieldTagHelper<IImageField>
    {
        private const string StyleAttributeName = "style";
        private readonly IFileUrlGenerator fileUrlGenerator;

        [HtmlAttributeName("content-image")]
        public override ModelExpression FieldName { get; set; }

        [HtmlAttributeName("content-image-width")]
        public int Width { get; set; }

        [HtmlAttributeName("content-image-height")]
        public int Height { get; set; }

        public ImageTagHelper(IFileUrlGenerator fileUrlGenerator)
        {
            this.fileUrlGenerator = fileUrlGenerator ?? throw new ArgumentNullException(nameof(fileUrlGenerator));
        }

        protected override async Task RenderContentAsync(TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;

            var value = Field.GetModelValue(Content);
            if (!Field.HasValue(value))
                return;
            var imageValue = (ImageValue)value;

            output.Attributes.TryGetAttribute(StyleAttributeName, out TagHelperAttribute styleAttribute);
            if (styleAttribute == null || styleAttribute.Value is not string styleStr)
                styleStr = string.Empty;
            if (!string.IsNullOrEmpty(styleStr) && !styleStr.EndsWith(";"))
                styleStr += ";";

            var imageUrl = await fileUrlGenerator.GetImageUrlAsync(imageValue, Width, Height);

            styleStr += $"background-image: url({imageUrl});";

            output.Attributes.SetAttribute(new TagHelperAttribute(StyleAttributeName, styleStr));

            if (IsDesigner)
            {
                output.Attributes.SetAttribute("content-image-width", Width);
                output.Attributes.SetAttribute("content-image-height", Height);
            }
        }
    }
}