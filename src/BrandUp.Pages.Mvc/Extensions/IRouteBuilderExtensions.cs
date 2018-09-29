using BrandUp.Pages.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    public static class IRouteBuilderExtensions
    {
        public static void MapPageRoute(this IRouteBuilder routes)
        {
            routes.MapRoute(
                name: RouteConstants.FileRouteName,
                template: RouteConstants.FileRouteUrlTemplate,
                defaults: new { controller = RouteConstants.FileControllerName, action = RouteConstants.FileControllerActionName }
            );

            routes.MapRoute(
                name: RouteConstants.DraftPageRouteName,
                template: RouteConstants.DraftPageRouteUrlTemplate,
                defaults: new { controller = RouteConstants.PageControllerName, action = RouteConstants.DraftPageActionName }
            );

            routes.MapRoute(
                name: RouteConstants.PublishedPageRouteName,
                template: RouteConstants.PublishedPageRouteUrlTemplate,
                defaults: new { controller = RouteConstants.PageControllerName, action = RouteConstants.PublishedPageActionName }
            );
        }
    }
}