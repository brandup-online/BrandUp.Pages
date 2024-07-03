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
        public static async Task<IHtmlContent> RenderContentAsync<TContent>(this IHtmlHelper htmlHelper, ContentScope scope, string contentKey, Func<TContent, Task> defaultFactory = null)
            where TContent : class
        {
            return await RenderContentAsync(htmlHelper, scope, contentKey, typeof(TContent), async content =>
            {
                if (defaultFactory != null)
                    await defaultFactory.Invoke((TContent)content);
            });
        }

        /// <summary>
        /// Render content by key.
        /// </summary>
        /// <param name="htmlHelper">Current html helper.</param>
        /// <param name="contentKey">Content key.</param>
        /// <param name="contentType">Content type.</param>
        /// <returns>Rendered html content.</returns>
        public static async Task<IHtmlContent> RenderContentAsync(this IHtmlHelper htmlHelper, ContentScope scope, string contentKey, Type contentType, Func<object, Task> defaultFactory = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(htmlHelper));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(contentKey));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(contentType));

            return await RenderContentAsync(htmlHelper, scope, new StaticContent(contentKey, contentType), defaultFactory);
        }

        /// <summary>
        /// Render content by item.
        /// </summary>
        /// <typeparam name="TItem">Item type.</typeparam>
        /// <param name="htmlHelper">Current html helper.</param>
        /// <param name="item">Item instance.</param>
        /// <returns>Rendered html content.</returns>
        public static async Task<IHtmlContent> RenderContentAsync<TItem>(this IHtmlHelper htmlHelper, ContentScope scope, TItem item, Func<object, Task> defaultFactory = null)
            where TItem : IItemContent
        {
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var cancellationToken = httpContext.RequestAborted;
            var services = httpContext.RequestServices;

            var itemType = MappingHelper.GetServiceKey<TItem>();
            var contentProvider = services.GetContentMappingProvider<TItem>();
            var contentKey = await contentProvider.GetContentKeyAsync(item, cancellationToken);
            var contentType = await contentProvider.GetContentTypeAsync(item, cancellationToken);
            var contentMetadataManager = services.GetRequiredService<ContentMetadataManager>();

            if (!contentMetadataManager.TryGetMetadata(contentType, out var contentMetadata))
                throw new InvalidOperationException($"Type {contentType.AssemblyQualifiedName} is not registered as content.");

            switch (scope)
            {
                case ContentScope.Website:
                    var websiteContext = services.GetRequiredService<IWebsiteContext>();
                    contentKey = $"{websiteContext.Website.Id}-{contentKey}";
                    break;
                case ContentScope.Global:
                    break;
                default:
                    throw new NotImplementedException();
            }

            var viewRenderService = services.GetRequiredService<IViewRenderService>();
            var contentService = services.GetRequiredService<ContentService>();
            var content = await contentService.FindContentAsync(contentKey, cancellationToken);
            content ??= await contentService.CreateAsync(itemType, item.ItemId, contentKey, cancellationToken);

            object contentModel;
            IContentEdit contentEdit;
            if (httpContext.IsEditContent(content, out var editContext))
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
                    contentModel = await contentService.CreateDefaultAsync(contentMetadata, cancellationToken);
                    contentModel ??= contentMetadata.CreateModelInstance();

                    await contentProvider.OnDefaultFactoryAsync(item.ItemId, contentModel, cancellationToken);

                    if (defaultFactory != null)
                        await defaultFactory.Invoke(contentModel);
                }

                contentEdit = null;
            }

            var contentContext = new ContentContext(contentKey, contentModel, services, contentEdit);

            var builder = new HtmlContentBuilder();
            var pageHtml = await viewRenderService.RenderToStringAsync(contentContext);
            builder.AppendHtml(pageHtml);
            return builder;
        }
    }

    public enum ContentScope
    {
        Global,
        Website
    }
}