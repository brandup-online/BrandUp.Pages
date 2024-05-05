using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
	[HtmlTargetElement(Attributes = "content-text")]
	public class TextTagHelper : FieldTagHelper<ITextField>
	{
		[HtmlAttributeName("content-text")]
		public override ModelExpression FieldName { get; set; }

		protected override Task RenderContentAsync(TagHelperOutput output)
		{
			var value = Field.GetModelValue(Content) as string;
			if (value != null)
				value = value.Replace("\n", "<br />");

			output.Content.SetHtmlContent(value ?? string.Empty);

			output.TagMode = TagMode.StartTagAndEndTag;

			return Task.CompletedTask;
		}
	}
}