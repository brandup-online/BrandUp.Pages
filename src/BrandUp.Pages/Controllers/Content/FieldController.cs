using System.ComponentModel.DataAnnotations;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Models.Contents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/content/[controller]"), ApiController, Filters.Administration]
    public abstract class FieldController<TField> : Controller, IAsyncActionFilter
        where TField : class, IFieldProvider
    {
        ContentService contentService;
        IContentEdit editSession;
        ContentContext contentContext;
        TField field;
        ContentContext rootContentContext;

        public IContentEdit ContentEdit => editSession;
        public TField Field => field;
        public ContentContext ContentContext => contentContext;

        #region IAsyncActionFilter members

        async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            contentService = HttpContext.RequestServices.GetRequiredService<ContentService>();

            if (!Request.Query.TryGetValue("editId", out Microsoft.Extensions.Primitives.StringValues editIdValue) || !Guid.TryParse(editIdValue[0], out Guid editId))
            {
                context.Result = BadRequest();
                return;
            }

            editSession = await contentService.FindEditByIdAsync(editId, HttpContext.RequestAborted);
            if (editSession == null)
            {
                context.Result = BadRequest();
                return;
            }

            var content = await contentService.GetEditContentAsync(editSession, HttpContext.RequestAborted);

            rootContentContext = new ContentContext(editSession.ContentKey, content, HttpContext.RequestServices, editSession);

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
        public async Task<IActionResult> GetAsync()
        {
            return await FormValueResultAsync();
        }

        protected async Task SaveChangesAsync()
        {
            await contentService.UpdateEditContentAsync(ContentEdit, rootContentContext.Content);
        }

        protected async Task<IActionResult> FormValueResultAsync()
        {
            var modelValue = Field.GetModelValue(contentContext.Content);
            var formValue = await Field.GetFormValueAsync(modelValue, HttpContext.RequestServices);

            var validationContext = new ValidationContext(contentContext.Content, HttpContext.RequestServices, null);
            var errors = Field.GetErrors(contentContext.Content, validationContext);

            return new JsonResult(new FieldValueResult
            {
                Value = formValue,
                Errors = errors
            });
        }
    }
}