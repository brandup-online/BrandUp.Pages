using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/content"), ApiController]
    public class ContentController : ControllerBase, IAsyncActionFilter
    {
        private IPageService pageService;
        private IPageEditingService pageEditingService;
        private IPage page;
        private IPageEditSession editSession;
        private ContentContext rootContentContext;
        private ContentContext contentContext;

        public IPage Page => page;
        public IPageEditSession ContentEdit => editSession;
        public ContentContext RootContentContext => rootContentContext;
        public ContentContext ContentContext => contentContext;

        #region IAsyncActionFilter members

        async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            pageService = HttpContext.RequestServices.GetRequiredService<IPageService>();
            pageEditingService = HttpContext.RequestServices.GetRequiredService<IPageEditingService>();

            if (!Request.Query.TryGetValue("editId", out Microsoft.Extensions.Primitives.StringValues editIdValue) || !Guid.TryParse(editIdValue[0], out Guid editId))
            {
                context.Result = BadRequest();
                return;
            }

            editSession = await pageEditingService.FindEditSessionById(editId);
            if (editSession == null)
            {
                context.Result = BadRequest();
                return;
            }

            page = await pageService.FindPageByIdAsync(editSession.PageId);
            if (page == null)
            {
                context.Result = BadRequest();
                return;
            }

            var content = await pageEditingService.GetContentAsync(editSession);

            rootContentContext = new ContentContext(page, content, HttpContext.RequestServices);

            string modelPath = string.Empty;
            if (Request.Query.TryGetValue("path", out Microsoft.Extensions.Primitives.StringValues pathValue))
                modelPath = pathValue[0];

            contentContext = rootContentContext.Navigate(modelPath);
            if (contentContext == null)
            {
                context.Result = BadRequest();
                return;
            }

            await next();
        }

        #endregion
    }
}