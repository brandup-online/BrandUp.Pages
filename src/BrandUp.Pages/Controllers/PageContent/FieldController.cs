using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/content/[controller]"), ApiController, Filters.Administration]
    public abstract class FieldController<TField> : Controller, IAsyncActionFilter
        where TField : class, IFieldProvider
    {
        private IPageService pageService;
        private IPageContentService pageEditingService;
        private IPage page;
        private IPageEdit editSession;
        private ContentContext contentContext;
        private TField @field;
        private ContentContext rootContentContext;

        public IPage Page => page;
        public IPageEdit ContentEdit => editSession;
        public TField Field => @field;
        public ContentContext ContentContext => contentContext;

        #region IAsyncActionFilter members

        async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            pageService = HttpContext.RequestServices.GetRequiredService<IPageService>();
            pageEditingService = HttpContext.RequestServices.GetRequiredService<IPageContentService>();

            if (!Request.Query.TryGetValue("editId", out Microsoft.Extensions.Primitives.StringValues editIdValue) || !Guid.TryParse(editIdValue[0], out Guid editId))
            {
                context.Result = BadRequest();
                return;
            }

            editSession = await pageEditingService.FindEditByIdAsync(editId, HttpContext.RequestAborted);
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

            var content = await pageEditingService.GetContentAsync(editSession, HttpContext.RequestAborted);

            rootContentContext = new ContentContext(page, content, HttpContext.RequestServices, true);

            string modelPath = string.Empty;
            if (Request.Query.TryGetValue("path", out Microsoft.Extensions.Primitives.StringValues pathValue))
                modelPath = pathValue[0];

            contentContext = rootContentContext.Navigate(modelPath);
            if (contentContext == null)
            {
                context.Result = BadRequest();
                return;
            }

            if (!Request.Query.TryGetValue("field", out Microsoft.Extensions.Primitives.StringValues fieldNameValue))
            {
                context.Result = BadRequest();
                return;
            }
            string fieldName = fieldNameValue[0];

            if (!contentContext.Explorer.Metadata.TryGetField(fieldName, out field))
            {
                context.Result = BadRequest();
                return;
            }

            await next();
        }

        #endregion

        [HttpGet]
        public Task<IActionResult> GetAsync()
        {
            return FormValueAsync();
        }

        protected async Task SaveChangesAsync()
        {
            await pageEditingService.SetContentAsync(ContentEdit, rootContentContext.Content);
        }

        protected async Task<IActionResult> FormValueAsync()
        {
            var modelValue = Field.GetModelValue(contentContext.Content);
            var formValue = await Field.GetFormValueAsync(modelValue, HttpContext.RequestServices);
            return new JsonResult(formValue);
        }
    }
}