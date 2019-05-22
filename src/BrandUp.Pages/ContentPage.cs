using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public abstract class ContentPage<TModel> : RazorPage<TModel>
    {
        public ContentContext Content => ViewData[Views.RazorViewRenderService.ViewData_ContentContextKeyName] as ContentContext;

        public async Task<IEnumerable<IPage>> GetChildPagesAsync(IPageCollectionReference pageCollectionReference)
        {
            var pageService = ViewContext.HttpContext.RequestServices.GetRequiredService<IPageService>();

            if (pageCollectionReference.CollectionId == Guid.Empty)
                return new IPage[0];

            var pages = await pageService.GetPagesAsync(new GetPagesOptions(pageCollectionReference.CollectionId));

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