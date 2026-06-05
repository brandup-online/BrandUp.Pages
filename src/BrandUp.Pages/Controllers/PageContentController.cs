using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
	[Route("brandup.pages/page/content"), Filters.Administration]
	public class PageContentController : Controller
	{
		readonly IPageService pageService;
		readonly IPageContentService pageContentService;
		readonly Url.IPageLinkGenerator pageLinkGenerator;

		public PageContentController(IPageService pageService, IPageContentService pageContentService, Url.IPageLinkGenerator pageLinkGenerator)
		{
			this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
			this.pageContentService = pageContentService ?? throw new ArgumentNullException(nameof(pageContentService));
			this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
		}

		[HttpPost("begin")]
		public async Task<IActionResult> BeginEditAsync([FromQuery] Guid pageId, [FromQuery] bool force)
		{
			var page = await pageService.FindPageByIdAsync(pageId);
			if (page == null)
				return NotFound();

			var result = new Models.BeginPageEditResult();

			var currentEdit = await pageContentService.FindEditByUserAsync(page, HttpContext.RequestAborted);
			if (currentEdit != null)
			{
				if (force)
				{
					await pageContentService.DiscardEditAsync(currentEdit, HttpContext.RequestAborted);
					currentEdit = null;
				}
				else
					result.CurrentDate = currentEdit.CreatedDate;
			}

			if (currentEdit == null)
				currentEdit = await pageContentService.BeginEditAsync(page, HttpContext.RequestAborted);

			result.Url = await pageLinkGenerator.GetPathAsync(currentEdit, HttpContext.RequestAborted);

			return Ok(result);
		}

		[HttpGet("form")]
		public async Task<IActionResult> GetFormAsync([FromQuery] Guid editId, [FromQuery] string modelPath)
		{
			var editSession = await pageContentService.FindEditByIdAsync(editId);
			if (editSession == null)
				return Conflict(); // session committed/discarded — stale edit conflict

			var page = await pageService.FindPageByIdAsync(editSession.PageId);
			if (page == null)
				return NotFound();

			if (modelPath == null)
				modelPath = string.Empty;

			var pageContent = await pageContentService.GetContentAsync(editSession);
			var pageContentContext = new ContentContext(page, pageContent, HttpContext.RequestServices, true);

			var contentContext = pageContentContext.Navigate(modelPath);
			if (contentContext == null)
				return BadRequest();

			var formModel = new Models.PageContentForm
			{
				Path = new Models.PageContentPath
				{
					Name = contentContext.Explorer.Metadata.Name,
					Title = contentContext.Explorer.Metadata.Title,
					Index = contentContext.Explorer.Index,
					ModelPath = contentContext.Explorer.ModelPath
				}
			};

			var path = formModel.Path;
			var explorer = contentContext.Explorer.Parent;
			while (explorer != null)
			{
				path.Parent = new Models.PageContentPath
				{
					Name = explorer.Metadata.Name,
					Title = explorer.Metadata.Title,
					Index = explorer.Index,
					ModelPath = explorer.ModelPath
				};

				explorer = explorer.Parent;
				path = path.Parent;
			}

			var fields = contentContext.Explorer.Metadata.Fields.ToList();
			foreach (var field in fields)
			{
				formModel.Fields.Add(new Models.ContentFieldModel
				{
					Type = field.Type,
					Name = field.Name,
					Title = field.Title,
					Options = field.GetFormOptions(contentContext.Services)
				});

				var modelValue = field.GetModelValue(contentContext.Content);
				var formValue = await field.GetFormValueAsync(modelValue, contentContext.Services);

				formModel.Values.Add(field.Name, formValue);
			}

			return Ok(formModel);
		}

		[HttpGet("changeType")]
		public async Task<IActionResult> ChangeModelTypeAsync([FromQuery] Guid editId, [FromQuery] string modelPath, [FromQuery] string modelType, [FromServices] IContentMetadataManager contentMetadataManager)
		{
			if (modelType == null)
				return BadRequest();

			var editSession = await pageContentService.FindEditByIdAsync(editId);
			if (editSession == null)
				return Conflict(); // session committed/discarded — stale edit conflict

			var page = await pageService.FindPageByIdAsync(editSession.PageId);
			if (page == null)
				return NotFound();

			if (modelPath == null)
				modelPath = string.Empty;

			var pageContent = await pageContentService.GetContentAsync(editSession);
			var pageContentExplorer = ContentExplorer.Create(contentMetadataManager, pageContent);

			var contentExplorer = pageContentExplorer.Navigate(modelPath);
			if (contentExplorer == null)
				return BadRequest();

			contentExplorer.Field!.ChangeType(contentExplorer.Model, modelType);

			// Сохраняем изменённую модель: ChangeType мутирует граф модели в pageContentExplorer.Model,
			// иначе изменение теряется при завершении запроса. SetContentAsync сам сериализует модель.
			await pageContentService.SetContentAsync(editSession, pageContentExplorer.Model);

			return Ok();
		}

		[HttpPost("commit")]
		public async Task<IActionResult> CommitEditAsync([FromQuery] Guid editId)
		{
			var editSession = await pageContentService.FindEditByIdAsync(editId);
			if (editSession == null)
				return NotFound();

			var page = await pageService.FindPageByIdAsync(editSession.PageId);
			if (page == null)
				return NotFound();

			await pageContentService.CommitEditAsync(editSession);

			return Ok(await pageLinkGenerator.GetPathAsync(page));
		}

		[HttpPost("discard")]
		public async Task<IActionResult> DiscardEditAsync([FromQuery] Guid editId)
		{
			var editSession = await pageContentService.FindEditByIdAsync(editId);
			if (editSession == null)
				return NotFound();

			var page = await pageService.FindPageByIdAsync(editSession.PageId);
			if (page == null)
				return NotFound();

			await pageContentService.DiscardEditAsync(editSession);

			return Ok(await pageLinkGenerator.GetPathAsync(page));
		}
	}
}