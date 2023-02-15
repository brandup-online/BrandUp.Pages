using BrandUp.Pages;
using BrandUp.Pages.Url;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    public static class IUrlHelperExtensions
    {
        public static Task<string> ItemPathAsync<TItem>(this IUrlHelper urlHelper, TItem item)
            where TItem : class
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return pageLinkGenerator.GetItemPathAsync(item, urlHelper.ActionContext.HttpContext.RequestAborted);
        }

        public static async Task<string> PagePathAsync(this IUrlHelper urlHelper, string pagePath)
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return await pageLinkGenerator.GetPathAsync(pagePath, urlHelper.ActionContext.HttpContext.RequestAborted);
        }

        public static async Task<string> PageUrlAsync(this IUrlHelper urlHelper, string pagePath)
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return await pageLinkGenerator.GetUrlAsync(pagePath, urlHelper.ActionContext.HttpContext.RequestAborted);
        }

        public static async Task<string> PagePathAsync(this IUrlHelper urlHelper, IPage page)
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return await pageLinkGenerator.GetPathAsync(page, urlHelper.ActionContext.HttpContext.RequestAborted);
        }

        public static async Task<string> PageUrlAsync(this IUrlHelper urlHelper, IPage page)
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return await pageLinkGenerator.GetUrlAsync(page, urlHelper.ActionContext.HttpContext.RequestAborted);
        }
    }
}