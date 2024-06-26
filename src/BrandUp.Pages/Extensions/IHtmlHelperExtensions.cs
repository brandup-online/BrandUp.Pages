using BrandUp.Pages.Content;
using BrandUp.Pages.Features;
using BrandUp.Pages.Pages;
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
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var viewRenderService = httpContext.RequestServices.GetRequiredService<IViewRenderService>();
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

            var httpContext = htmlHelper.ViewContext.HttpContext;
            var cancellationToken = httpContext.RequestAborted;
            var services = httpContext.RequestServices;
            var contentMetadataManager = services.GetRequiredService<ContentMetadataManager>();
            var viewLocator = services.GetRequiredService<IViewLocator>();

            if (!contentMetadataManager.TryGetMetadata(contentType, out var contentMetadataProvider))
                throw new InvalidOperationException($"Type {contentType.AssemblyQualifiedName} is not registered as content.");

            var viewRenderService = services.GetRequiredService<IViewRenderService>();
            var websiteContext = services.GetRequiredService<IWebsiteContext>();

            object contentModel;
            IContentEdit contentEdit = null;
            var contentEditFeature = httpContext.Features.Get<ContentEditFeature>();
            if (contentEditFeature != null && contentEditFeature.IsEdit(key))
            {
                contentModel = contentEditFeature.Content;
                contentEdit = contentEditFeature.Edit;
            }
            else
            {
                var contentService = services.GetRequiredService<ContentService>();
                contentModel = await contentService.GetContentAsync(websiteContext.Website.Id, key, cancellationToken);
                if (contentModel == null)
                {
                    contentModel = await contentService.CreateDefaultAsync(contentMetadataProvider, cancellationToken);
                    contentModel ??= contentMetadataProvider.CreateModelInstance();
                }
            }

            var contentContext = new ContentContext(key, contentModel, services, contentEdit);

            var builder = new HtmlContentBuilder();
            var pageHtml = await viewRenderService.RenderToStringAsync(contentContext);
            builder.AppendHtml(pageHtml);
            return builder;
        }
    }
}