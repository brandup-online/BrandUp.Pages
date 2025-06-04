using BrandUp.Pages.Content;
using BrandUp.Pages.Features;
using BrandUp.Website.Pages.Results;
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
            var accessProvider = httpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();

            if (await accessProvider.CheckAccessAsync(cancellationToken))
            {
                var contentService = httpContext.RequestServices.GetRequiredService<ContentService>();
                if (httpContext.Request.Query.TryGetValue("editid", out string editIdValue) && Guid.TryParse(editIdValue, out var editId))
                {
                    var editSession = await contentService.FindEditByIdAsync(editId, cancellationToken);
                    if (editSession == null)
                    {
                        context.Result = new NotFoundResult();
                        return;
                    }

                    var userId = await accessProvider.GetUserIdAsync(cancellationToken);
                    if (!editSession.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.Result = new NotFoundResult();
                        return;
                    }

                    var content = await contentService.GetEditContentAsync(editSession, cancellationToken);

                    httpContext.Features.Set(new ContentEditFeature(editSession, content));
                }
                else if (httpContext.Request.Query.TryGetValue("bpcommand", out string commandName) && httpContext.Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
                {
                    commandName = commandName.ToLower();
                    switch (commandName)
                    {
                        case "begin":
                            httpContext.Request.QueryString.Add("editid", "");

                            context.Result = new PageRedirectResult("") { Replace = true };
                            break;
                        case "commit":
                            context.Result = new BadRequestResult();
                            break;
                        case "discard":
                            context.Result = new BadRequestResult();
                            break;
                        default:
                            context.Result = new BadRequestResult();
                            break;
                    }

                    return;
                }
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
}