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
            var viewRenderService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IViewRenderService>();
            var viewLocator = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IViewLocator>();
            var contentMetadataManager = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IContentMetadataManager>();
            var pageService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageService>();
            var pageContentService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageContentService>();

            if (!contentMetadataManager.TryGetMetadata(modelType, out var blockMetadata))
                throw new InvalidOperationException(); //todo сообщение


            if (htmlHelper.ViewData.Model is not AppPageModel model)
                throw new InvalidOperationException(); //todo сообщение

            string pagePath = string.Empty;
            var routeData = model.RouteData;
            if (routeData.Values.TryGetValue("page", out object urlValue) && urlValue != null)
                pagePath = (string)urlValue;
            var websiteId = model.WebsiteContext.Website.Id;

            object pageContent = null;
            var page = await pageService.FindPageByPathAsync(websiteId, pagePath + key);
            if (page == null)
            {
                var view = viewLocator.FindView(modelType);
                pageContent = blockMetadata.ConvertDictionaryToContentModel(view.DefaultModelData);

                page = await pageService.FindPageByPathAsync(websiteId, "index"); // Переделать

                // todo создать страницу 
            }
            else
            {
                pageContent = pageService.GetPageContentAsync(page);
            }

            var builder = new HtmlContentBuilder();
            var context = new ContentContext(page, pageContent, htmlHelper.ViewContext.HttpContext.RequestServices, false); // todo флаг
            var content = await viewRenderService.RenderToStringAsync(context);
            builder.AppendHtml(content);

            return builder;
        }
    }
}