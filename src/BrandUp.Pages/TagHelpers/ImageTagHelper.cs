using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Content.Files;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement(Attributes = "content-image")]
    public class ImageTagHelper : FieldTagHelper<IImageField>
    {
        private const string StyleAttributeName = "style";
        private readonly IFileUrlGenerator fileUrlGenerator;

        [HtmlAttributeName("content-image")]
        public override ModelExpression FieldName { get; set; }

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
            if (styleAttribute == null || !(styleAttribute.Value is string styleStr))
                styleStr = string.Empty;
            if (!string.IsNullOrEmpty(styleStr) && !styleStr.EndsWith(";"))
                styleStr += ";";

            var imageUrl = await fileUrlGenerator.GetImageUrlAsync(imageValue);

            styleStr += $"background-image: url({imageUrl});";

            output.Attributes.SetAttribute(new TagHelperAttribute(StyleAttributeName, styleStr));
        }
    }
}