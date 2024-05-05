using BrandUp.Pages.Content.Fields;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
	[HtmlTargetElement(Attributes = "content-html")]
	public class HtmlTagHelper : FieldTagHelper<IHtmlField>
	{
		[HtmlAttributeName("content-html")]
		public override ModelExpression FieldName { get; set; }

		protected override Task RenderContentAsync(TagHelperOutput output)
		{
			var value = Field.GetModelValue(Content) as string;

			output.Content.SetHtmlContent(value ?? string.Empty);

			output.TagMode = TagMode.StartTagAndEndTag;

			return Task.CompletedTask;
		}
	}
}