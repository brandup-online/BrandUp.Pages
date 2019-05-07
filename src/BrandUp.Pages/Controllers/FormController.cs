using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [ApiController]
    public abstract class FormController<TForm, TValues, TResult> : ControllerBase
        where TForm : Models.FormModel<TValues>, new()
        where TValues : class, new()
        where TResult : class, new()
    {
        [HttpGet]
        public async Task<IActionResult> GetFormAsync([FromRoute]string id)
        {
            await OnInitializeAsync(id);

            if (!ModelState.IsValid)
                return ValidationProblem();

            var formModel = new TForm();

            await OnBuildFormAsync(formModel);

            return Ok(formModel);
        }

        [HttpPut]
        public async Task<IActionResult> ChangeValueAsync([FromRoute]string id, [FromQuery]string field, [FromBody]TValues values)
        {
            await OnInitializeAsync(id);

            await OnChangeValueAsync(field, values);

            if (!ModelState.IsValid)
                return ValidationProblem();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CommitAsync([FromRoute]string id, [FromBody]TValues values)
        {
            await OnInitializeAsync(id);

            if (!TryValidateModel(values))
                return ValidationProblem();

            var resultModel = await OnCommitAsync(values);

            if (!ModelState.IsValid)
                return ValidationProblem();

            return Ok(resultModel);
        }

        protected abstract Task OnInitializeAsync(string id);
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

    public abstract class FormController<TValues, TResult> : FormController<Models.FormModel<TValues>, TValues, TResult>
        where TValues : class, new()
        where TResult : class, new()
    {
    }
}