using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Url;
using BrandUp.Website;
using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
	public sealed class ContentPageModel : AppPageModel
	{
		IPage page = null!;
		IPageEdit? editSession;
		PageSeoOptions pageSeo = null!;

		#region Properties

		[FromQuery(Name = "editId"), ClientProperty]
		public Guid? EditId { get; set; }
		public IPageService PageService { get; private set; } = null!;
		public IPage PageEntry => page;
		public PageMetadataProvider PageMetadata { get; private set; } = null!;
		public object PageContent { get; private set; } = null!;
		public ContentContext ContentContext { get; private set; } = null!;
		[ClientProperty]
		public Guid Id => page.Id;
		[ClientProperty]
		public Models.PageStatus Status { get; private set; }
		[ClientProperty]
		public Guid? ParentPageId { get; private set; }

		#endregion

		#region AppPageModel members

		public override string Title => !string.IsNullOrEmpty(pageSeo.Title) ? pageSeo.Title : PageMetadata.GetPageHeader(PageContent);
		public override string? Description => pageSeo.Description;
		public override string? Keywords => pageSeo.Keywords != null ? string.Join(",", pageSeo.Keywords) : null;
		public override string ScriptName => "content";
		protected override async Task OnPageRequestAsync(PageRequestContext context)
		{
			PageService = HttpContext.RequestServices.GetRequiredService<IPageService>();

			if (EditId.HasValue)
			{
				var pageEditingService = HttpContext.RequestServices.GetRequiredService<IPageContentService>();
				editSession = await pageEditingService.FindEditByIdAsync(EditId.Value);
				if (editSession == null)
				{
					// The edit URL is always "?editId=..." over the default (home) content page,
					// so a stale editId falls back to the home page.
					var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
					context.Result = Redirect(await pageLinkGenerator.GetPathAsync(string.Empty));
					return;
				}

				var editPage = await PageService.FindPageByIdAsync(editSession.PageId);
				if (editPage == null)
				{
					context.Result = NotFound();
					return;
				}
				page = editPage;

				var accessProvider = HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
				if (!await accessProvider.CheckAccessAsync() || await accessProvider.GetUserIdAsync() != editSession.UserId)
				{
					var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();

					context.Result = RedirectPermanent(await pageLinkGenerator.GetPathAsync(page));
					return;
				}
			}
			else
			{
				var routeData = RouteData;

				var pagePath = string.Empty;
				if (routeData.Values.TryGetValue("url", out object? urlValue) && urlValue != null)
					pagePath = (string)urlValue;

				var url = await PageService.FindUrlByPathAsync(WebsiteContext.Website.Id, pagePath);
				if (url == null)
				{
					context.Result = NotFound();
					return;
				}

				if (url.PageId.HasValue)
				{
					var urlPage = await PageService.FindPageByIdAsync(url.PageId.Value);
					if (urlPage == null)
					{
						context.Result = NotFound();
						return;
					}
					page = urlPage;

					if (!page.IsPublished)
					{
						var accessProvider = HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
						if (!await accessProvider.CheckAccessAsync())
						{
							context.Result = NotFound();
							return;
						}
					}
				}
				else
				{
					if (url.Redirect == null)
					{
						context.Result = NotFound();
						return;
					}

					var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
					var redirectUrl = await pageLinkGenerator.GetPathAsync(url.Redirect.Path);

					if (url.Redirect.IsPermament)
						context.Result = RedirectPermanent(redirectUrl);
					else
						context.Result = Redirect(redirectUrl);
					return;
				}
			}

			PageMetadata = await PageService.GetPageTypeAsync(page, HttpContext.RequestAborted);

			pageSeo = await PageService.GetPageSeoOptionsAsync(page, HttpContext.RequestAborted);

			if (editSession != null)
			{
				var pageEditingService = HttpContext.RequestServices.GetRequiredService<IPageContentService>();
				PageContent = await pageEditingService.GetContentAsync(editSession, HttpContext.RequestAborted);
			}
			else
				PageContent = await PageService.GetPageContentAsync(page, HttpContext.RequestAborted);
			if (PageContent == null)
				throw new InvalidOperationException();

			ContentContext = new ContentContext(page, PageContent, HttpContext.RequestServices, editSession != null);

			Status = page.IsPublished ? Models.PageStatus.Published : Models.PageStatus.Draft;
			ParentPageId = await PageService.GetParentPageIdAsync(page, HttpContext.RequestAborted);
		}

		#endregion
	}
}