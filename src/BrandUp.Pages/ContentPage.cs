using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
	public abstract class ContentPage<TModel> : RazorPage<TModel>
	{
		public ContentContext Content => ViewData[Views.RazorViewRenderService.ViewData_ContentContextKeyName] as ContentContext;
		public IPage Page => Content.Page;

		public Task<IEnumerable<IPage>> GetChildPagesAsync(IPageCollectionReference pageCollectionReference)
		{
			return GetChildPagesAsync(pageCollectionReference, -1, -1);
		}
		public Task<IEnumerable<IPage>> GetChildPagesAsync(IPageCollectionReference pageCollectionReference, int limit)
		{
			return GetChildPagesAsync(pageCollectionReference, 0, limit);
		}
		public async Task<IEnumerable<IPage>> GetChildPagesAsync(IPageCollectionReference pageCollectionReference, int offset, int limit)
		{
			var pageService = ViewContext.HttpContext.RequestServices.GetRequiredService<IPageService>();

			if (pageCollectionReference.CollectionId == Guid.Empty)
				return new IPage[0];

			var options = new GetPagesOptions(pageCollectionReference.CollectionId);
			if (offset >= 0 && limit > 0)
				options.Pagination = new PagePaginationOptions(offset, limit);

			var accessProvider = ViewContext.HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
			if (await accessProvider.CheckAccessAsync())
				options.IncludeDrafts = true;

			var pages = await pageService.GetPagesAsync(options);

			return pages;
		}

		public async Task<IEnumerable<IPage>> EachParentPagesAsync(IPage page, bool includeCurrentPage = false)
		{
			var pageService = ViewContext.HttpContext.RequestServices.GetRequiredService<IPageService>();
			var result = new List<IPage>();
			if (includeCurrentPage)
				result.Add(page);

			IPage currentPage = page;
			while (currentPage != null)
			{
				var parentPageId = await pageService.GetParentPageIdAsync(currentPage);
				if (!parentPageId.HasValue)
					break;

				currentPage = await pageService.FindPageByIdAsync(parentPageId.Value);
				result.Add(currentPage);
			}

			result.Reverse();

			return result;
		}
		public Task<IEnumerable<IPage>> EachParentPagesAsync(bool includeCurrentPage = false)
		{
			return EachParentPagesAsync(Content.Page, includeCurrentPage);
		}
	}
}