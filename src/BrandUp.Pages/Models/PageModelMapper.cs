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
			IPage? page = null;
			if (pageCollection.PageId.HasValue)
				page = await pageService.FindPageByIdAsync(pageCollection.PageId.Value);

			return await pageCollection.ToViewModelAsync(page, pageLinkGenerator);
		}

		/// <summary>
		/// Маппинг списка коллекций без N+1: страницы-владельцы загружаются по уникальным
		/// идентификаторам (для коллекций одной страницы это, как правило, один запрос).
		/// </summary>
		public static async Task<List<PageCollectionModel>> ToViewModelsAsync(this IEnumerable<IPageCollection> pageCollections, IPageService pageService, IPageLinkGenerator pageLinkGenerator)
		{
			var collections = pageCollections as IReadOnlyCollection<IPageCollection> ?? pageCollections.ToList();

			var pageCache = new Dictionary<Guid, IPage>();
			foreach (var pageId in collections.Where(it => it.PageId.HasValue).Select(it => it.PageId!.Value).Distinct())
			{
				var cachedPage = await pageService.FindPageByIdAsync(pageId);
				if (cachedPage != null)
					pageCache[pageId] = cachedPage;
			}

			var result = new List<PageCollectionModel>(collections.Count);
			foreach (var pageCollection in collections)
			{
				IPage? page = null;
				if (pageCollection.PageId.HasValue)
					pageCache.TryGetValue(pageCollection.PageId.Value, out page);

				result.Add(await pageCollection.ToViewModelAsync(page, pageLinkGenerator));
			}

			return result;
		}

		static async Task<PageCollectionModel> ToViewModelAsync(this IPageCollection pageCollection, IPage? page, IPageLinkGenerator pageLinkGenerator)
		{
			var pageUrl = page != null ? await pageLinkGenerator.GetPathAsync(page) : "/";

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
