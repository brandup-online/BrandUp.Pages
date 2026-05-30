using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Url;

namespace BrandUp.Pages.Models
{
	/// <summary>
	/// Единые мапперы доменных сущностей в модели представления — чтобы не дублировать
	/// преобразование IPage/IPageCollection по контроллерам.
	/// </summary>
	static class PageModelMapper
	{
		public static async Task<PageModel> ToViewModelAsync(this IPage page, IPageLinkGenerator pageLinkGenerator)
		{
			return new PageModel
			{
				Id = page.Id,
				CreatedDate = page.CreatedDate,
				Title = page.Header,
				Status = page.IsPublished ? PageStatus.Published : PageStatus.Draft,
				Url = await pageLinkGenerator.GetPathAsync(page)
			};
		}

		public static async Task<PageCollectionModel> ToViewModelAsync(this IPageCollection pageCollection, IPageService pageService, IPageLinkGenerator pageLinkGenerator)
		{
			var pageUrl = "/";
			if (pageCollection.PageId.HasValue)
			{
				var page = await pageService.FindPageByIdAsync(pageCollection.PageId.Value);
				pageUrl = await pageLinkGenerator.GetPathAsync(page);
			}

			return new PageCollectionModel
			{
				Id = pageCollection.Id,
				CreatedDate = pageCollection.CreatedDate,
				PageId = pageCollection.PageId,
				Title = pageCollection.Title,
				PageType = pageCollection.PageTypeName,
				Sort = pageCollection.SortMode,
				CustomSorting = pageCollection.CustomSorting,
				PageUrl = pageUrl
			};
		}
	}
}
