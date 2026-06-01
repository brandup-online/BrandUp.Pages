using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Models;
using BrandUp.Website;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
	[Route("brandup.pages/collection/list", Name = "BrandUp.Pages.Collection.List"), Filters.Administration]
	public class PageCollectionListController : ListController<PageCollectionListModel, PageCollectionModel, IPageCollection, Guid>
	{
		readonly IPageService pageService;
		readonly IPageCollectionService pageCollectionService;
		readonly Url.IPageLinkGenerator pageLinkGenerator;
		readonly IWebsiteContext websiteContext;
		private IPage? page;

		public PageCollectionListController(IPageService pageService, IPageCollectionService pageCollectionService, Url.IPageLinkGenerator pageLinkGenerator, IWebsiteContext websiteContext)
		{
			this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
			this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
			this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
			this.websiteContext = websiteContext ?? throw new ArgumentNullException(nameof(websiteContext));
		}

		#region ListController members

		protected override async Task OnInitializeAsync()
		{
			if (Request.Query.TryGetValue("pageId", out string? pageIdValue))
			{
				if (!Guid.TryParse(pageIdValue, out Guid pageId))
				{
					AddErrors("Not valid id.");
					return;
				}

				page = await pageService.FindPageByIdAsync(pageId);
				if (page == null)
				{
					AddErrors("Not found page.");
					return;
				}
			}
		}

		protected override async Task OnBuildListAsync(PageCollectionListModel listModel)
		{
			listModel.Parents = new List<string>();

			if (page != null)
			{
				listModel.Parents.Add(page.Header);

				IPage? currentPage = page;
				while (currentPage != null)
				{
					var parentPageId = await pageService.GetParentPageIdAsync(currentPage);
					if (!parentPageId.HasValue)
						break;

					currentPage = await pageService.FindPageByIdAsync(parentPageId.Value);
					if (currentPage == null)
						break;

					listModel.Parents.Add(currentPage.Header);
				}

				listModel.Parents.Reverse();
			}
		}

		protected override Guid ParseId(string value)
		{
			return Guid.Parse(value);
		}

		protected override Task<IEnumerable<IPageCollection>> OnGetItemsAsync(int offset, int limit)
		{
			if (page != null)
				return pageCollectionService.ListCollectionsAsync(page);
			else
				return pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id);
		}

		protected override async Task<IPageCollection> OnGetItemAsync(Guid id)
		{
			return await pageCollectionService.FindCollectiondByIdAsync(id) ?? throw new InvalidOperationException("Collection not found.");
		}

		protected override Task<PageCollectionModel> OnGetItemModelAsync(IPageCollection item)
			=> item.ToViewModelAsync(pageService, pageLinkGenerator);
		protected override async Task<IEnumerable<PageCollectionModel>> OnGetItemModelsAsync(IEnumerable<IPageCollection> items)
			=> await items.ToViewModelsAsync(pageService, pageLinkGenerator);

		#endregion
	}
}