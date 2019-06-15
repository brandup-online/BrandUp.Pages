using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [ApiController]
    public abstract class ListController<TListModel, TItemModel, TItem, TId> : Controller
        where TListModel : class, new()
        where TItemModel : class, new()
        where TItem : class
    {
        [HttpGet]
        public async Task<IActionResult> GetListAsync()
        {
            await OnInitializeAsync();

            if (!ModelState.IsValid)
                return ValidationProblem();

            var formModel = new TListModel();

            await OnBuildListAsync(formModel);

            return Ok(formModel);
        }

        [HttpGet, Route("item")]
        public async Task<IActionResult> GetItemsAsync()
        {
            await OnInitializeAsync();

            if (!ModelState.IsValid)
                return ValidationProblem();

            var result = new List<TItemModel>();

            var items = await OnGetItemsAsync(0, 50);
            if (items == null)
            {
                AddErrors();
                return ValidationProblem();
            }

            foreach (var item in items)
            {
                var itemModel = await OnGetItemModelAsync(item);
                result.Add(itemModel);
            }

            return Ok(result);
        }

        [HttpGet("item/{id}")]
        public async Task<IActionResult> GetItemsAsync([FromRoute]string id)
        {
            await OnInitializeAsync();

            if (!ModelState.IsValid)
                return ValidationProblem();

            var idValue = ParseId(id);

            var item = await OnGetItemAsync(idValue);
            if (item == null)
                return NotFound();

            var itemModel = await OnGetItemModelAsync(item);
            return Ok(itemModel);
        }

        protected abstract Task OnInitializeAsync();
        protected abstract Task OnBuildListAsync(TListModel listModel);
        protected abstract TId ParseId(string value);
        protected abstract Task<IEnumerable<TItem>> OnGetItemsAsync(int offset, int limit);
        protected abstract Task<TItem> OnGetItemAsync(TId id);
        protected abstract Task<TItemModel> OnGetItemModelAsync(TItem item);

        protected void AddErrors(IResult result)
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
}