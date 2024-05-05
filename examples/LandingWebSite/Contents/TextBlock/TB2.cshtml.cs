using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.TextBlock
{
	[ContentType(Title = "Текст с заголовком")]
	public class TB2 : TextBlockContent
	{
		[Text(DisplayBeforeField = nameof(Text))]
		public string Header { get; set; }
	}
}