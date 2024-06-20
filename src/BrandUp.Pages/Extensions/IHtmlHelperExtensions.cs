using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Views;
using BrandUp.Website;
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

        /// <summary>
        /// Рендеринг статического блока.
        /// </summary>
        /// <param name="htmlHelper">HTML хелпер представления.</param>
        /// <param name="contentType">Тип контента блока.</param>
        /// <param name="key">Ключ блока.</param>
        /// <returns>HTML контент блока.</returns>
        public static async Task<IHtmlContent> BlockAsync(this IHtmlHelper htmlHelper, Type contentType, string key)
        {
            ArgumentNullException.ThrowIfNull(nameof(htmlHelper));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(contentType));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(key));

            var services = htmlHelper.ViewContext.HttpContext.RequestServices;
            var contentMetadataManager = services.GetRequiredService<IContentMetadataManager>();
            var viewLocator = services.GetRequiredService<IViewLocator>();
            var viewRenderService = services.GetRequiredService<IViewRenderService>();

            if (!contentMetadataManager.TryGetMetadata(contentType, out var contentMetadataProvider))
                throw new InvalidOperationException("Не зарегистрирован тип контента.");

            var view = viewLocator.FindView(contentMetadataProvider.ModelType) ?? throw new InvalidOperationException();

            var contentModel = FindContentModelAsync(services, htmlHelper.ViewContext, contentMetadataProvider, key);
            if (contentModel == null && view.DefaultModelData != null)
                contentMetadataProvider.ApplyDataToModel(view.DefaultModelData, contentModel);

            var contentContext = new ContentContext(null, contentModel, services, false);

            var builder = new HtmlContentBuilder();
            var pageHtml = await viewRenderService.RenderToStringAsync(contentContext);
            builder.AppendHtml(pageHtml);
            return builder;
        }

        static async Task<object> FindContentModelAsync(IServiceProvider services, ViewContext viewContext, ContentMetadataProvider contentMetadataProvider, string key, CancellationToken cancellationToken = default)
        {
            var pageService = services.GetRequiredService<IPageService>();

            var routeData = viewContext.RouteData;
            string pagePath = string.Empty;
            if (routeData.Values.TryGetValue("page", out object urlValue) && urlValue != null)
                pagePath = ((string)urlValue).Trim('/');

            var pageUrl = pagePath + "\\" + key;
            var websiteId = viewContext.HttpContext.GetWebsiteContext().Website.Id;

            var content = await pageService.GetPageContentAsync(websiteId, pageUrl, cancellationToken);
            if (content != null)
                return content;

            return contentMetadataProvider.CreateModelInstance();
        }
    }
}