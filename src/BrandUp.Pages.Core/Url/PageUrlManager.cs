using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BrandUp.Pages.Url
{
    public class PageUrlManager : IPageUrlManager
    {
        private readonly HttpContext httpContext;

        public PageUrlManager(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public string GetPageUrl(IPage page)
        {
            string path;
            if (httpContext.Request.PathBase.HasValue)
                path = httpContext.Request.PathBase.Value;
            else
                path = "/";

            if (page.UrlPath != null)
            {
                if (page.UrlPath != "home")
                    path += page.UrlPath;
            }
            else
                path += "_draft/" + page.Id;

            return path;
        }
    }
}