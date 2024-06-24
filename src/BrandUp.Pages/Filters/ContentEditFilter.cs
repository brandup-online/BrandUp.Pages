using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Filters
{
    /// <summary>
    /// Detect content edit session by page.
    /// </summary>
    public class ContentEditFilter : IAsyncPageFilter, IOrderedFilter
    {
        #region IAsyncPageFilter members

        async Task IAsyncPageFilter.OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var cancellationToken = httpContext.RequestAborted;

            if (httpContext.Request.Query.TryGetValue("editid", out string editIdValue) && Guid.TryParse(editIdValue, out var editId))
            {
                var contentEditService = httpContext.RequestServices.GetRequiredService<IContentEditService>();
                var editSession = await contentEditService.FindEditByIdAsync(editId, cancellationToken);
                if (editSession == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var accessProvider = httpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
                if (!await accessProvider.CheckAccessAsync(cancellationToken) || await accessProvider.GetUserIdAsync(cancellationToken) != editSession.UserId)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var content = await contentEditService.GetContentAsync(editSession, cancellationToken);

                httpContext.Features.Set(new ContentEditFeature(editSession, content));
            }

            await next();
        }

        Task IAsyncPageFilter.OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region IOrderedFilter members

        int IOrderedFilter.Order => int.MinValue;

        #endregion
    }

    internal class ContentEditFeature
    {
        public IContentEdit Edit { get; }
        public object Content { get; }

        public ContentEditFeature(IContentEdit contentEdit, object content)
        {
            Edit = contentEdit;
            Content = content;
        }

        public bool IsEdit(string contentKey)
        {
            ArgumentNullException.ThrowIfNull(contentKey);

            return contentKey.Equals(Edit.ContentKey, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}