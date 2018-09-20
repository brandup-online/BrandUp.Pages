namespace BrandUp.Pages.Mvc
{
    public static class RouteConstants
    {
        public static readonly string PageControllerName = "WebSite";

        public static readonly string PublishedPageRouteName = "BrandUp.WebSite.PublishedPageRoute";
        public static readonly string PublishedPageActionName = "Page";
        public static readonly string PublishedPagePathRouteValueKey = "pagepath";
        public static readonly string PublishedPageRouteUrlTemplate = "{*" + PublishedPagePathRouteValueKey + "}";

        public static readonly string DraftPageRouteName = "BrandUp.WebSite.DraftPageRoute";
        public static readonly string DraftPageActionName = "Draft";
        public static readonly string DraftPagePathRouteValueKey = "pageId";
        public static readonly string DraftPageRouteUrlTemplate = "_draft/{" + DraftPagePathRouteValueKey + "}";

        public static readonly string FileControllerName = "File";
        public static readonly string FileControllerActionName = "Get";
        public static readonly string FileRouteName = "BrandUp.WebSite.File";
        public static readonly string FileIdValueKey = "fileId";
        public static readonly string FileRouteUrlTemplate = "_file/{" + FileIdValueKey + "}";
    }
}