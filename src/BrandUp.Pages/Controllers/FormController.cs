using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [ApiController, FormController]
    public abstract class FormController<TForm, TValues, TResult> : ControllerBase
        where TForm : Models.FormModel<TValues>, new()
        where TValues : class, new()
        where TResult : class, new()
    {
        [HttpGet]
        public async Task<IActionResult> GetFormAsync()
        {
            await OnInitializeAsync();

            if (!ModelState.IsValid)
                return ValidationProblem();

            var formModel = new TForm();

            await OnBuildFormAsync(formModel);

            return Ok(formModel);
        }

        [HttpPut]
        public async Task<IActionResult> ChangeValueAsync([FromQuery]string field, [FromBody]TValues values)
        {
            await OnInitializeAsync();

            await OnChangeValueAsync(field, values);

            if (!ModelState.IsValid)
                return ValidationProblem();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CommitAsync([FromBody]TValues values)
        {
            await OnInitializeAsync();

            if (!TryValidateModel(values))
                return ValidationProblem();

            var resultModel = await OnCommitAsync(values);

            if (!ModelState.IsValid)
                return ValidationProblem();

            return Ok(resultModel);
        }

        protected abstract Task OnInitializeAsync();
        protected abstract Task OnBuildFormAsync(TForm formModel);
        protected abstract Task OnChangeValueAsync(string field, TValues values);
        protected abstract Task<TResult> OnCommitAsync(TValues values);

        protected void AddErrors(Result result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
        }
        protected void AddErrors(params string[] errors)
        {
            foreach (var error in errors)
                ModelState.AddModelError(string.Empty, error);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class FormControllerAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                context.ModelState.AddModelError(string.Empty, ex.Message);
                context.Result = ((ControllerBase)context.Controller).ValidationProblem();
            }
        }
    }

    public abstract class FormController<TValues, TResult> : FormController<Models.FormModel<TValues>, TValues, TResult>
        where TValues : class, new()
        where TResult : class, new()
    {
    }
}