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

            var httpContext = context.HttpContext;
            var services = httpContext.RequestServices;
            var cancellationToken = httpContext.RequestAborted;
            var contentMetadataManager = services.GetRequiredService<ContentMetadataManager>();
            var pageModelType = pageModel.GetType();
            foreach (var @interface in pageModelType.GetInterfaces())
            {
                if (!@interface.IsConstructedGenericType)
                    continue;

                var gType = @interface.GetGenericTypeDefinition();
                if (gType == typeof(IContentPage<>))
                {
                    var contentType = @interface.GenericTypeArguments[0];
                    var contentMetadata = contentMetadataManager.GetMetadata(contentType);

                    var contentKeyProperty = @interface.GetProperty("ContentKey", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var contentKey = (string)contentKeyProperty.GetValue(pageModel);

                    var itemContent = new StaticContent(contentKey, contentType);
                    var itemTypeKey = MappingHelper.GetServiceKey<StaticContent>();
                    var contentProvider = services.GetContentMappingProvider<StaticContent>();

                    var contentService = httpContext.RequestServices.GetRequiredService<ContentService>();
                    var content = await contentService.FindContentAsync(contentKey, cancellationToken);
                    content ??= await contentService.CreateAsync(itemTypeKey, itemContent.ItemId, contentKey, cancellationToken);

                    object contentModel;
                    IContentEdit contentEdit;
                    if (httpContext.IsEditContent(content, out var editContext))
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

                            await contentProvider.OnDefaultFactoryAsync(itemContent.ItemId, contentModel, cancellationToken);
                        }

                        contentEdit = null;
                    }

                    var contentProperty = @interface.GetProperty("ContentModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    contentProperty.SetValue(pageModel, contentModel);

                    var contentContext = new ContentContext(contentKey, contentModel, httpContext.RequestServices, contentEdit);
                    pageModel.ViewData.Add(RazorViewRenderService.ViewData_ContentContextKeyName, contentContext);

                    var contentRenderingContext = new ContentRenderingContext();
                    pageModel.ViewData.Add(RazorViewRenderService.ViewData_ViewRenderingContextKeyName, contentRenderingContext);

                    pageModel.ViewData.Add(RazorViewRenderService.ViewData_ContentPageContextKeyName, new ContentPageContext());

                    break;
                }
                else if (gType == typeof(IContentPage<,>))
                {
                    var itemType = @interface.GenericTypeArguments[0];
                    var itemTypeKey = MappingHelper.GetServiceKey(itemType);
                    var contentProvider = services.GetContentMappingProvider(itemType);

                    var itemContentProperty = @interface.GetProperty("ContentItem", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var itemContent = (IItemContent)itemContentProperty.GetValue(pageModel);

                    var contentKey = await contentProvider.GetContentKeyAsync(itemContent, cancellationToken);
                    var contentType = await contentProvider.GetContentTypeAsync(itemContent, cancellationToken);
                    var contentMetadata = contentMetadataManager.GetMetadata(contentType);

                    var contentService = httpContext.RequestServices.GetRequiredService<ContentService>();
                    var content = await contentService.FindContentAsync(contentKey, cancellationToken);
                    content ??= await contentService.CreateAsync(itemTypeKey, itemContent.ItemId, contentKey, cancellationToken);

                    object contentModel;
                    IContentEdit contentEdit;
                    if (httpContext.IsEditContent(content, out var editContext))
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

                            await contentProvider.OnDefaultFactoryAsync(itemContent.ItemId, contentModel, cancellationToken);
                        }

                        contentEdit = null;
                    }

                    var contentProperty = @interface.GetProperty("ContentModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    contentProperty.SetValue(pageModel, contentModel);

                    var contentContext = new ContentContext(contentKey, contentModel, httpContext.RequestServices, contentEdit);
                    pageModel.ViewData.Add(RazorViewRenderService.ViewData_ContentContextKeyName, contentContext);

                    var contentRenderingContext = new ContentRenderingContext();
                    pageModel.ViewData.Add(RazorViewRenderService.ViewData_ViewRenderingContextKeyName, contentRenderingContext);

                    pageModel.ViewData.Add(RazorViewRenderService.ViewData_ContentPageContextKeyName, new ContentPageContext());

                    break;
                }
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
    }
}