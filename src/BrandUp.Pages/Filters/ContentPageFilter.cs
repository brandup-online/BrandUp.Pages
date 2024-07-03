using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Items;
using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Filters
{
    public class ContentPageFilter : IAsyncPageFilter, IOrderedFilter
    {
        #region IAsyncPageFilter members

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (context.HandlerInstance is not PageModel pageModel)
            {
                await next();
                return;
            }

            if (TryGetContentPage(pageModel, out var item, out var contentType))
            {
                var contentModel = await RenderContentAsync(pageModel, item, contentType, context.HttpContext.RequestAborted);

                var c = typeof(IContentPage<>).MakeGenericType(contentType);

                var contentProperty = c.GetProperty("ContentModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                contentProperty.SetValue(pageModel, contentModel);
            }

            await next();
        }

        public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            await Task.CompletedTask;
        }

        #endregion

        #region IOrderedFilter members

        int IOrderedFilter.Order => int.MinValue + 1;

        #endregion

        #region Helpers

        static bool TryGetContentPage(PageModel pageModel, out IItemContent item, out Type contentType)
        {
            var pageModelType = pageModel.GetType();
            foreach (var @interface in pageModelType.GetInterfaces())
            {
                if (!@interface.IsConstructedGenericType)
                    continue;

                var gType = @interface.GetGenericTypeDefinition();
                if (gType == typeof(IStaticContentPage<>))
                {
                    contentType = @interface.GenericTypeArguments[0];

                    var contentKeyProperty = @interface.GetProperty("ContentKey", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var contentKey = (string)contentKeyProperty.GetValue(pageModel);

                    item = new StaticContent(contentKey);
                    return true;
                }
                else if (gType == typeof(IItemContentPage<,>))
                {
                    contentType = @interface.GenericTypeArguments[1];

                    var itemContentProperty = @interface.GetProperty("ContentItem", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    item = (IItemContent)itemContentProperty.GetValue(pageModel);
                    return true;
                }
            }

            item = default;
            contentType = default;
            return false;
        }

        static async Task<object> RenderContentAsync(PageModel pageModel, IItemContent item, Type contentType, CancellationToken cancellationToken)
        {
            var services = pageModel.HttpContext.RequestServices;

            var itemType = item.GetType();
            var itemTypeKey = MappingHelper.GetServiceKey(itemType);

            var contentProvider = services.GetContentMappingProvider(itemType);

            var contentMetadataManager = services.GetRequiredService<ContentMetadataManager>();
            var contentMetadata = contentMetadataManager.GetMetadata(contentType);
            var contentKey = await contentProvider.GetContentKeyAsync(item, cancellationToken);

            var contentService = services.GetRequiredService<ContentService>();
            var content = await contentService.FindContentAsync(contentKey, cancellationToken);
            content ??= await contentService.CreateAsync(itemTypeKey, item.ItemId, contentKey, cancellationToken);

            object contentModel;
            IContentEdit contentEdit;
            if (pageModel.HttpContext.IsEditContent(content, out var editContext))
            {
                contentModel = await contentService.GetEditContentAsync(editContext.Edit, cancellationToken);
                contentEdit = editContext.Edit;
            }
            else
            {
                if (content.CommitId != null)
                {
                    var contentData = await contentService.GetContentAsync(content.CommitId, cancellationToken);
                    contentModel = contentData.Data;
                }
                else
                {
                    contentModel = await contentService.CreateDefaultAsync(contentMetadata, cancellationToken);
                    contentModel ??= contentMetadata.CreateModelInstance();

                    await contentProvider.OnDefaultFactoryAsync(item.ItemId, contentModel, cancellationToken);
                }

                contentEdit = null;
            }

            var contentContext = new ContentContext(contentKey, contentModel, services, contentEdit);
            pageModel.ViewData.Add(RazorViewRenderService.ViewData_ContentContextKeyName, contentContext);

            var contentRenderingContext = new ContentRenderingContext();
            pageModel.ViewData.Add(RazorViewRenderService.ViewData_ViewRenderingContextKeyName, contentRenderingContext);

            pageModel.ViewData.Add(RazorViewRenderService.ViewData_ContentPageContextKeyName, new ContentPageContext());

            return contentModel;
        }

        #endregion
    }
}