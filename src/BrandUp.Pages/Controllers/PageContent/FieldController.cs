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
        private IPageService pageService = null!;
        private IPageContentService pageEditingService = null!;
        private IPage page = null!;
        private IPageEdit editSession = null!;
        private ContentContext contentContext = null!;
        private TField @field = null!;
        private ContentContext rootContentContext = null!;

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

            var loadedEdit = await pageEditingService.FindEditByIdAsync(editId, HttpContext.RequestAborted);
            if (loadedEdit == null)
            {
                context.Result = BadRequest();
                return;
            }
            editSession = loadedEdit;

            var loadedPage = await pageService.FindPageByIdAsync(editSession.PageId);
            if (loadedPage == null)
            {
                context.Result = BadRequest();
                return;
            }
            page = loadedPage;

            var content = await pageEditingService.GetContentAsync(editSession, HttpContext.RequestAborted);
            if (content == null)
            {
                context.Result = BadRequest();
                return;
            }

            rootContentContext = new ContentContext(page, content, HttpContext.RequestServices, true);

            string modelPath = string.Empty;
            if (Request.Query.TryGetValue("path", out Microsoft.Extensions.Primitives.StringValues pathValue))
                modelPath = pathValue[0] ?? string.Empty;

            var navContext = rootContentContext.Navigate(modelPath);
            if (navContext == null)
            {
                context.Result = BadRequest();
                return;
            }
            contentContext = navContext;

            if (!Request.Query.TryGetValue("field", out Microsoft.Extensions.Primitives.StringValues fieldNameValue))
            {
                context.Result = BadRequest();
                return;
            }
            string? fieldName = fieldNameValue[0];
            if (fieldName == null)
            {
                context.Result = BadRequest();
                return;
            }

            if (!contentContext.Explorer.Metadata.TryGetField<TField>(fieldName, out var foundField))
            {
                context.Result = BadRequest();
                return;
            }
            @field = foundField;

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