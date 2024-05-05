using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Services
{
	public static class IPageServiceExtensions
	{
		public static Task<IPage> CreatePageAsync<TContent>(this IPageService pageService, IPageCollection collection, TContent pageContent, CancellationToken cancellationToken = default)
			where TContent : class
		{
			return pageService.CreatePageAsync(collection, pageContent, cancellationToken);
		}
	}
}