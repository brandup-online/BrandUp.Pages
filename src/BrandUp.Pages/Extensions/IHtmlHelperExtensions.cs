using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Views;
using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    public static class IHtmlHelperExtensions
    {
        public static async Task<IHtmlContent> RenderPageAsync(this IHtmlHelper<ContentPageModel> htmlHelper)
        {
            var viewRenderService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IViewRenderService>();
            var pageModel = htmlHelper.ViewData.Model;

            var builder = new HtmlContentBuilder();
            var pageHtml = await viewRenderService.RenderToStringAsync(pageModel.ContentContext);
            builder.AppendHtml(pageHtml);
            return builder;
        }
        public static async Task<IHtmlContent> BlockAsync(this IHtmlHelper htmlHelper, Type modelType, string key)
        {
            var services = htmlHelper.ViewContext.HttpContext.RequestServices;

            var viewRenderService = services.GetRequiredService<IViewRenderService>();
            var pageService = services.GetRequiredService<IPageService>();

            if (htmlHelper.ViewData.Model is not AppPageModel model)
                throw new InvalidOperationException(); //todo сообщение

            var page = await GetPageAsync(services, model, modelType, key);
            var pageContent = await pageService.GetPageContentAsync(page);

            var builder = new HtmlContentBuilder();
            var context = new ContentContext(page, pageContent, htmlHelper.ViewContext.HttpContext.RequestServices, false); // todo флаг
            var content = await viewRenderService.RenderToStringAsync(context);
            builder.AppendHtml(content);

            return builder;
        }

        #region BlockAsync helpers

        static async Task<IPage> GetPageAsync(IServiceProvider services, AppPageModel pageModel, Type pageContentType, string key)
        {
            var pageService = services.GetRequiredService<IPageService>();

            string pagePath = string.Empty;
            var routeData = pageModel.RouteData;
            if (routeData.Values.TryGetValue("page", out object urlValue) && urlValue != null)
                pagePath = ((string)urlValue).Trim('/');

            var pageUrl = pagePath + "\\" + key;
            var websiteId = pageModel.WebsiteContext.Website.Id;

            var page = await pageService.FindPageByPathAsync(websiteId, pageUrl);
            if (page != null)
                return page;

            var pageContent = GetContent(services, pageContentType);

            var collection = await GetPageCollectionAsync(services, pageModel.WebsiteContext.Website.Id, pageModel.Title, "commonpage");
            page = await pageService.CreatePageAsync(collection, pageContent);

            var publishResult = await pageService.PublishPageAsync(page, pageUrl);
            if (!publishResult.IsSuccess)
                throw new InvalidOperationException($"Не удалось опубликовать страницу.{publishResult.Errors.FirstOrDefault()}");

            return page;
        }

        static async Task<IPageCollection> GetPageCollectionAsync(IServiceProvider services, string websiteId, string title, string pageTypeName)
        {
            var pageCollectionService = services.GetRequiredService<IPageCollectionService>();

            var collections = await pageCollectionService.FindCollectionsAsync(websiteId, pageTypeName, title, false);
            if (collections != null)
            {
                var collection = collections.FirstOrDefault();
                if (collection != null)
                    return collection;
            }

            var collectionResult = await pageCollectionService.CreateCollectionAsync(websiteId, title, pageTypeName, PageSortMode.FirstNew);
            if (!collectionResult.IsSuccess)
                throw new Exception();

            return collectionResult.Data;
        }

        static object GetContent(IServiceProvider services, Type modelType)
        {
            var viewLocator = services.GetRequiredService<IViewLocator>();
            var contentMetadataManager = services.GetRequiredService<IContentMetadataManager>();

            if (!contentMetadataManager.TryGetMetadata(modelType, out var blockMetadata))
                throw new InvalidOperationException(); //todo сообщение

            var view = viewLocator.FindView(modelType);

            return blockMetadata.ConvertDictionaryToContentModel(view.DefaultModelData);
        }

        #endregion
    }
}