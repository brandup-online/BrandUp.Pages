using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace LandingWebSite.Contents.Page
{
	[PageContent]
	public abstract class PageContent
	{
		[Text(Placeholder = "Input page header"), Title]
		public string Header { get; set; }
	}
}