using BrandUp.Pages.Url;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    public static class IUrlHelperExtensions
    {
        public static async Task<string> PagePathAsync(this IUrlHelper urlHelper, string path)
        {
            var pageLinkGenerator = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
            return await pageLinkGenerator.GetPathAsync(path);
        }
    }
}