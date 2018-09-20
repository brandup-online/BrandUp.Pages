using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Mvc;
using Microsoft.AspNetCore.Routing;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string WebSitePage(this IUrlHelper urlHelper, string pagePath)
        {
            return WebSitePage(urlHelper, pagePath, null);
        }

        public static string WebSitePage(this IUrlHelper urlHelper, string pagePath, object query)
        {
            if (urlHelper == null)
                throw new ArgumentNullException(nameof(urlHelper));
            if (pagePath == null)
                throw new ArgumentNullException(nameof(pagePath));

            return urlHelper.RouteUrl(RouteConstants.PublishedPageRouteName, new RouteValueDictionary(query) { { RouteConstants.PublishedPagePathRouteValueKey, pagePath } });
        }

        public static string WebSitePage(this IUrlHelper urlHelper, IPage page)
        {
            return WebSitePage(urlHelper, page, null);
        }

        public static string WebSitePage(this IUrlHelper urlHelper, IPage page, object query)
        {
            if (urlHelper == null)
                throw new ArgumentNullException(nameof(urlHelper));
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (page.UrlPath != null)
                return urlHelper.RouteUrl(RouteConstants.PublishedPageRouteName, new RouteValueDictionary(query) { { RouteConstants.PublishedPagePathRouteValueKey, page.UrlPath } });
            else
                return urlHelper.RouteUrl(RouteConstants.DraftPageRouteName, new RouteValueDictionary(query) { { RouteConstants.DraftPagePathRouteValueKey, page.Id } });
        }

        public static string WebSiteFile(this IUrlHelper urlHelper, IPageFile file)
        {
            return WebSiteFile(urlHelper, file.Id);
        }
        public static string WebSiteFile(this IUrlHelper urlHelper, Guid fileId)
        {
            return urlHelper.RouteUrl(RouteConstants.FileRouteName, new RouteValueDictionary() { { RouteConstants.FileIdValueKey, fileId } });
        }

        public static string MainPage(this IUrlHelper urlHelper)
        {
            return WebSitePage(urlHelper, string.Empty);
        }
    }
}