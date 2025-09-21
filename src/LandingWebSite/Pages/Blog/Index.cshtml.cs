using BrandUp.Website.Pages;

namespace LandingWebSite.Pages.Blog
{
	public class IndexModel : AppPageModel
	{
		public override string Title => "About";
		public override string Description => "About page description";
		public override string Keywords => "about, company";
		public override string ScriptName => "about";
		public override string CssClass => "about-page";

		protected override Task OnPageRequestAsync(PageRequestContext context)
		{
			//SetOpenGraph(Url.ContentLink("~/images/banner.jpg"), Title, Description);

			return base.OnPageRequestAsync(context);
		}
	}
}