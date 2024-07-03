using BrandUp.Pages.Features;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    public static class RazorPageExtensions
    {
        public static Task<IEnumerable<IPage>> GetChildPagesAsync(this RazorPageBase razorPage, IPageCollectionReference pageCollectionReference)
        {
            return GetChildPagesAsync(razorPage, pageCollectionReference, -1, -1);
        }

        public static Task<IEnumerable<IPage>> GetChildPagesAsync(this RazorPageBase razorPage, IPageCollectionReference pageCollectionReference, int limit)
        {
            return GetChildPagesAsync(razorPage, pageCollectionReference, 0, limit);
        }

        public static async Task<IEnumerable<IPage>> GetChildPagesAsync(this RazorPageBase razorPage, IPageCollectionReference pageCollectionReference, int offset, int limit)
        {
            ArgumentNullException.ThrowIfNull(pageCollectionReference);

            var pageService = razorPage.ViewContext.HttpContext.RequestServices.GetRequiredService<PageService>();

            if (pageCollectionReference.CollectionId == Guid.Empty)
                return [];

            var options = new GetPagesOptions(pageCollectionReference.CollectionId);
            if (offset >= 0 && limit > 0)
                options.Pagination = new PagePaginationOptions(offset, limit);

            var accessProvider = razorPage.ViewContext.HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
            if (await accessProvider.CheckAccessAsync())
                options.IncludeDrafts = true;

            var pages = await pageService.GetPagesAsync(options);

            return pages;
        }

        public static async Task<IEnumerable<IPage>> EachParentPagesAsync(this RazorPageBase razorPage, IPage page, bool includeCurrentPage = false)
        {
            ArgumentNullException.ThrowIfNull(page);

            var pageService = razorPage.ViewContext.HttpContext.RequestServices.GetRequiredService<PageService>();
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

        public static async Task<IEnumerable<IPage>> EachParentPagesAsync(this RazorPageBase razorPage, bool includeCurrentPage = false)
        {
            var contentPageFeature = razorPage.ViewContext.HttpContext.Features.Get<ContentPageFeature>();
            if (contentPageFeature != null)
                return await EachParentPagesAsync(razorPage, contentPageFeature.Page, includeCurrentPage);
            else
                return [];
        }
    }
}
