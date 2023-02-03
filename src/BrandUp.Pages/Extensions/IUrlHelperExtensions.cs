using BrandUp.Pages;
using BrandUp.Pages.Url;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    public static class IUrlHelperExtensions
    {
        public static async Task<string> PagePathAsync(this IUrlHelper urlHelper, string pagePath)
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return await pageLinkGenerator.GetPathAsync(pagePath);
        }

        public static async Task<string> PageUrlAsync(this IUrlHelper urlHelper, string pagePath)
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return await pageLinkGenerator.GetUrlAsync(pagePath);
        }

        public static async Task<string> PagePathAsync(this IUrlHelper urlHelper, IPage page)
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return await pageLinkGenerator.GetPathAsync(page);
        }

        public static async Task<string> PageUrlAsync(this IUrlHelper urlHelper, IPage page)
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return await pageLinkGenerator.GetUrlAsync(page);
        }
    }
}