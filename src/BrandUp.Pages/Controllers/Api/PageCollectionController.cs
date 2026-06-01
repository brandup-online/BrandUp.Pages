using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Models;
using BrandUp.Website;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
	[ApiController, Filters.Administration]
	public class PageCollectionController : ControllerBase
	{
		readonly IPageCollectionService pageCollectionService;
		readonly IPageService pageService;
		readonly Url.IPageLinkGenerator pageLinkGenerator;
		readonly IWebsiteContext websiteContext;

		public PageCollectionController(IPageCollectionService pageCollectionService, IPageService pageService, Url.IPageLinkGenerator pageLinkGenerator, IWebsiteContext websiteContext)
		{
			this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
			this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
			this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
			this.websiteContext = websiteContext ?? throw new ArgumentNullException(nameof(websiteContext));
		}

		#region Action methods

		[HttpGet, Route("brandup.pages/collection/{id}", Name = "BrandUp.Pages.Collection.Get")]
		public async Task<IActionResult> GetAsync([FromRoute] Guid id)
		{
			var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(id);
			if (pageCollection == null)
				return NotFound();

			var model = await GetItemModelAsync(pageCollection);

			return Ok(model);
		}

		[HttpGet, Route("brandup.pages/collection/{id}/pageTypes", Name = "BrandUp.Pages.Collection.GetPageTypes")]
		public async Task<IActionResult> GetPageTypesAsync([FromRoute] Guid id)
		{
			var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(id);
			if (pageCollection == null)
				return NotFound();

			var result = new List<Models.PageTypeModel>();
			foreach (var pageType in await pageCollectionService.GetPageTypesAsync(pageCollection))
			{
				result.Add(new Models.PageTypeModel
				{
					Name = pageType.Name,
					Title = pageType.Title
				});
			}
			return Ok(result);
		}

		[HttpGet, Route("brandup.pages/collection", Name = "BrandUp.Pages.Collection.Items")]
		public async Task<IActionResult> ListAsync([FromQuery] Guid? pageId)
		{
			IEnumerable<IPageCollection> collections;
			if (pageId.HasValue)
			{
				var page = await pageService.FindPageByIdAsync(pageId.Value);
				if (page == null)
					return NotFound();
				collections = await pageCollectionService.ListCollectionsAsync(page);
			}
			else
			{
				collections = await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id);
			}

			var result = await collections.ToViewModelsAsync(pageService, pageLinkGenerator);

			return Ok(result);
		}

		[HttpGet, Route("brandup.pages/collection/search", Name = "BrandUp.Pages.Collection.Search")]
		public async Task<IActionResult> SearchAsync([FromQuery] string pageType, [FromQuery] string? title = null)
		{
			if (pageType == null)
				return BadRequest();

			var collections = await pageCollectionService.FindCollectionsAsync(websiteContext.Website.Id, pageType, title, true);

			var result = await collections.ToViewModelsAsync(pageService, pageLinkGenerator);

			return Ok(result);
		}

		[HttpDelete, Route("brandup.pages/collection/{id}", Name = "BrandUp.Pages.Collection.Delete")]
		public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
		{
			var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(id);
			if (pageCollection == null)
				return WithResult(Result.Failed($"Not found page collection with id \"{id}\"."));

			var deleteResult = await pageCollectionService.DeleteCollectionAsync(pageCollection, HttpContext.RequestAborted);

			return WithResult(deleteResult);
		}

		#endregion

		#region Helper methods

		private Task<Models.PageCollectionModel> GetItemModelAsync(IPageCollection pageCollection)
			=> pageCollection.ToViewModelAsync(pageService, pageLinkGenerator);
		private IActionResult WithResult(Result result)
		{
			if (result == null)
				throw new ArgumentNullException(nameof(result));

			if (result.IsSuccess)
				return Ok();
			else
				return BadRequest(result);
		}

		#endregion
	}
}