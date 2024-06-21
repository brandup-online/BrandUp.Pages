using BrandUp.Pages.Content;
using BrandUp.Pages.Views;
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
        /// Ренреринг статического блока.
        /// </summary>
        /// <param name="htmlHelper">HTML хелпер представления.</param>
        /// <param name="key">Ключ блока.</param>
        /// <param name="contentType">Тип контента блока.</param>
        /// <returns>HTML контент блока.</returns>
        public static async Task<IHtmlContent> BlockAsync(this IHtmlHelper htmlHelper, string key, Type contentType)
        {
            ArgumentNullException.ThrowIfNull(nameof(htmlHelper));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(key));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(contentType));

            var services = htmlHelper.ViewContext.HttpContext.RequestServices;
            var contentMetadataManager = services.GetRequiredService<IContentMetadataManager>();
            var viewLocator = services.GetRequiredService<IViewLocator>();
            var viewRenderService = services.GetRequiredService<IViewRenderService>();

            if (!contentMetadataManager.TryGetMetadata(contentType, out var contentMetadataProvider))
                throw new InvalidOperationException("Не зарегистрирован тип контента.");

            var view = viewLocator.FindView(contentMetadataProvider.ModelType);
            if (view == null)
                throw new InvalidOperationException();

            var contentModel = contentMetadataProvider.CreateModelInstance();

            if (view.DefaultModelData != null)
                contentMetadataProvider.ApplyDataToModel(view.DefaultModelData, contentModel);

            var contentContext = new ContentContext(key, contentModel, services, false);

            var builder = new HtmlContentBuilder();
            var pageHtml = await viewRenderService.RenderToStringAsync(contentContext);
            builder.AppendHtml(pageHtml);
            return builder;
        }
    }
}