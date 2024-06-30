using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Items;
using BrandUp.Pages.Views;
using BrandUp.Website;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    public static class IHtmlHelperExtensions
    {
        /// <summary>
        /// Render content by key.
        /// </summary>
        /// <param name="htmlHelper">Current html helper.</param>
        /// <param name="contentKey">Content key.</param>
        /// <param name="contentType">Content type.</param>
        /// <returns>Rendered html content.</returns>
        public static async Task<IHtmlContent> RenderContentAsync(this IHtmlHelper htmlHelper, string contentKey, Type contentType)
        {
            ArgumentNullException.ThrowIfNull(nameof(htmlHelper));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(contentKey));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(contentType));

            var httpContext = htmlHelper.ViewContext.HttpContext;
            var cancellationToken = httpContext.RequestAborted;
            var services = httpContext.RequestServices;
            var contentMetadataManager = services.GetRequiredService<ContentMetadataManager>();

            if (!contentMetadataManager.TryGetMetadata(contentType, out var contentProvider))
                throw new InvalidOperationException($"Type {contentType.AssemblyQualifiedName} is not registered as content.");

            var viewRenderService = services.GetRequiredService<IViewRenderService>();
            var websiteContext = services.GetRequiredService<IWebsiteContext>();
            var contentService = services.GetRequiredService<ContentService>();

            var content = await contentService.FindContentAsync(websiteContext.Website.Id, contentKey, cancellationToken);

            object contentModel;
            IContentEdit contentEdit;
            if (content != null && httpContext.IsEditContent(content, out var editContext))
            {
                contentModel = editContext.Content;
                contentEdit = editContext.Edit;
            }
            else
            {
                if (content != null && content.CommitId != null)
                {
                    var contentData = await contentService.GetContentAsync(content.CommitId, cancellationToken);
                    contentModel = contentData.Data;
                }
                else
                {
                    contentModel = await contentService.CreateDefaultAsync(contentProvider, cancellationToken);
                    contentModel ??= contentProvider.CreateModelInstance();
                }

                contentEdit = null;
            }

            var contentContext = new ContentContext(contentKey, contentModel, services, contentEdit);

            var builder = new HtmlContentBuilder();
            var pageHtml = await viewRenderService.RenderToStringAsync(contentContext);
            builder.AppendHtml(pageHtml);
            return builder;
        }

        /// <summary>
        /// Render content by item.
        /// </summary>
        /// <typeparam name="TItem">Item type.</typeparam>
        /// <param name="htmlHelper">Current html helper.</param>
        /// <param name="item">Item instance.</param>
        /// <returns>Rendered html content.</returns>
        public static async Task<IHtmlContent> RenderContentAsync<TItem>(this IHtmlHelper htmlHelper, TItem item)
            where TItem : IItemContent
        {
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var itemContentProvider = httpContext.RequestServices.GetRequiredService<IItemContentProvider<TItem>>();
            var itemContentKey = await itemContentProvider.GetContentKeyAsync(item, httpContext.RequestAborted);
            var itemContentType = await itemContentProvider.GetContentTypeAsync(item, httpContext.RequestAborted);

            return await RenderContentAsync(htmlHelper, itemContentKey, itemContentType);
        }
    }
}