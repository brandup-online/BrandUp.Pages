using BrandUp.Pages.Content;
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
        /// Ренреринг статического блока.
        /// </summary>
        /// <param name="htmlHelper">HTML хелпер представления.</param>
        /// <param name="key">Ключ блока.</param>
        /// <param name="contentType">Тип контента блока.</param>
        /// <returns>HTML контент блока.</returns>
        public static async Task<IHtmlContent> RenderContentAsync(this IHtmlHelper htmlHelper, string key, Type contentType)
        {
            ArgumentNullException.ThrowIfNull(nameof(htmlHelper));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(key));
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

            var content = await contentService.FindContentByKeyAsync(websiteContext.Website.Id, key, cancellationToken);

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

            var contentContext = new ContentContext(key, contentModel, services, contentEdit);

            var builder = new HtmlContentBuilder();
            var pageHtml = await viewRenderService.RenderToStringAsync(contentContext);
            builder.AppendHtml(pageHtml);
            return builder;
        }
    }
}