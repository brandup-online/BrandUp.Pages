using Microsoft.AspNetCore.Mvc;

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
		public async Task<IActionResult> GetItemsAsync([FromQuery] int offset = 0, [FromQuery] int limit = 50)
		{
			if (offset < 0)
				offset = 0;
			if (limit <= 0 || limit > 200)
				limit = 50;

			await OnInitializeAsync();

			if (!ModelState.IsValid)
				return ValidationProblem();

			var result = new List<TItemModel>();

			var items = await OnGetItemsAsync(offset, limit);
			if (items == null)
			{
				AddErrors();
				return ValidationProblem();
			}

			result.AddRange(await OnGetItemModelsAsync(items));

			return Ok(result);
		}
		[HttpGet("item/{id}")]
		public async Task<IActionResult> GetItemsAsync([FromRoute] string id)
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
		[HttpPost, Route("sort")]
		public async Task<IActionResult> SortAsync([FromQuery] string sourceId, [FromQuery] string destId, [FromQuery] ListItemSortPosition destPosition)
		{
			await OnInitializeAsync();

			if (!ModelState.IsValid)
				return ValidationProblem();

			var sId = ParseId(sourceId);
			var dId = ParseId(destId);

			var sourceItem = await OnGetItemAsync(sId);
			if (sourceItem == null)
				return BadRequest();

			var destItem = await OnGetItemAsync(dId);
			if (destItem == null)
				return BadRequest();

			await OnSortAsync(sourceItem, destItem, destPosition);

			return Ok();
		}

		protected abstract Task OnInitializeAsync();
		protected abstract Task OnBuildListAsync(TListModel listModel);
		protected abstract TId ParseId(string value);
		protected abstract Task<IEnumerable<TItem>> OnGetItemsAsync(int offset, int limit);
		protected abstract Task<TItem> OnGetItemAsync(TId id);
		protected abstract Task<TItemModel> OnGetItemModelAsync(TItem item);
		// Маппинг всего списка одним вызовом — точка расширения для батч-загрузки (без N+1).
		// По умолчанию — поэлементно через OnGetItemModelAsync.
		protected virtual async Task<IEnumerable<TItemModel>> OnGetItemModelsAsync(IEnumerable<TItem> items)
		{
			var result = new List<TItemModel>();
			foreach (var item in items)
				result.Add(await OnGetItemModelAsync(item));
			return result;
		}
		protected virtual Task OnSortAsync(TItem sourceItem, TItem destItem, ListItemSortPosition position)
		{
			return Task.CompletedTask;
		}

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

	public enum ListItemSortPosition
	{
		After,
		Before
	}
}