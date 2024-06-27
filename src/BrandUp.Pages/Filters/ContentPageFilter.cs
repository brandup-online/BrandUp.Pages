﻿using BrandUp.Pages.Content;
using BrandUp.Pages.Features;
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

                    object contentModel;
                    IContentEdit contentEdit = null;
                    var contentEditFeature = httpContext.Features.Get<ContentEditFeature>();
                    if (contentEditFeature != null && contentEditFeature.IsEdit(contentKey))
                    {
                        contentModel = contentEditFeature.Content;
                        contentEdit = contentEditFeature.Edit;
                    }
                    else
                    {
                        var contentService = httpContext.RequestServices.GetRequiredService<ContentService>();
                        contentModel = await contentService.GetContentAsync(websiteContext.Website.Id, contentKey, httpContext.RequestAborted);
                        if (contentModel == null)
                        {
                            contentModel = await contentService.CreateDefaultAsync(contentProvider, httpContext.RequestAborted);
                            contentModel ??= contentProvider.CreateModelInstance();
                        }
                    }

                    var contentProperty = @interface.GetProperty("ContentModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    contentProperty.SetValue(pageModel, contentModel);

                    var contentContext = new ContentContext(contentKey, contentModel, httpContext.RequestServices, contentEdit);
                    pageModel.ViewData.Add(Views.RazorViewRenderService.ViewData_ContentContextKeyName, contentContext);

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