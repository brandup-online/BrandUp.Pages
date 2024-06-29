using BrandUp.Pages.Content;
using BrandUp.Pages.Views;
using BrandUp.Website;
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
            var cancellationToken = httpContext.RequestAborted;
            var contentMetadataManager = httpContext.RequestServices.GetRequiredService<ContentMetadataManager>();
            var websiteContext = httpContext.RequestServices.GetRequiredService<IWebsiteContext>();
            var pageModelType = pageModel.GetType();
            foreach (var @interface in pageModelType.GetInterfaces())
            {
                if (!@interface.IsConstructedGenericType)
                    continue;

                var gType = @interface.GetGenericTypeDefinition();
                if (gType == typeof(IContentPage<>))
                {
                    var contentType = @interface.GenericTypeArguments[0];
                    var contentProvider = contentMetadataManager.GetMetadata(contentType);

                    var keyProperty = @interface.GetProperty("ContentKey", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    var contentKey = (string)keyProperty.GetValue(pageModel);
                    var contentService = httpContext.RequestServices.GetRequiredService<ContentService>();
                    var content = await contentService.FindContentByKeyAsync(websiteContext.Website.Id, contentKey, cancellationToken);

                    object contentModel;
                    IContentEdit contentEdit;
                    if (content != null && httpContext.IsEditContent(content, out var editContext))
                    {
                        contentModel = await contentService.GetEditContentAsync(editContext.Edit, cancellationToken);
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

                    var contentProperty = @interface.GetProperty("ContentModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    contentProperty.SetValue(pageModel, contentModel);

                    var contentContext = new ContentContext(contentKey, contentModel, httpContext.RequestServices, contentEdit);
                    pageModel.ViewData.Add(RazorViewRenderService.ViewData_ContentContextKeyName, contentContext);

                    var contentRenderingContext = new ContentRenderingContext();
                    pageModel.ViewData.Add(RazorViewRenderService.ViewData_ViewRenderingContextKeyName, contentRenderingContext);

                    break;
                }
            }

            var c = await next();
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