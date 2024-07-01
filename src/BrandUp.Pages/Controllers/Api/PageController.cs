using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Services;
using BrandUp.Pages.Url;
using BrandUp.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BrandUp.Pages.Controllers
{
    [ApiController, Filters.Administration]
    public class PageController(PageCollectionService pageCollectionService, PageService pageService, IPageLinkGenerator pageLinkGenerator, IWebsiteContext websiteContext) : ControllerBase
    {
        #region Action methods

        [HttpGet, Route("brandup.pages/page/{id}", Name = "BrandUp.Pages.Page.Get")]
        public async Task<IActionResult> GetAsync([FromRoute] Guid id)
        {
            var page = await pageService.FindPageByIdAsync(id);
            if (page == null)
                return NotFound();

            var model = await GetItemModelAsync(page);

            return Ok(model);
        }

        [HttpGet, Route("brandup.pages/page", Name = "BrandUp.Pages.Page.Items")]
        public async Task<IActionResult> ListAsync([FromQuery] Guid collectionId)
        {
            var collection = await pageCollectionService.FindCollectiondByIdAsync(collectionId);
            if (collection == null)
                return BadRequest();

            var result = new List<Models.PageModel>();

            var pages = await pageService.GetPagesAsync(new GetPagesOptions(collection.Id) { IncludeDrafts = true }, HttpContext.RequestAborted);
            foreach (var page in pages)
                result.Add(await GetItemModelAsync(page));

            return Ok(result);
        }

        [HttpGet, Route("brandup.pages/page/search", Name = "BrandUp.Pages.Page.Search")]
        public async Task<IActionResult> SearchAsync([FromQuery] string title)
        {
            var result = new List<Models.PageModel>();

            var pages = await pageService.SearchPagesAsync(websiteContext.Website.Id, title, new PagePaginationOptions(0, 20), HttpContext.RequestAborted);
            foreach (var page in pages)
                result.Add(await GetItemModelAsync(page));

            return Ok(result);
        }

        [HttpDelete, Route("brandup.pages/page/{id}", Name = "BrandUp.Pages.Page.Delete")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var page = await pageService.FindPageByIdAsync(id);
            if (page == null)
                return WithResult(Result.Failed($"Not found page with id \"{id}\"."));

            var deleteResult = await pageService.DeletePageAsync(page, HttpContext.RequestAborted);

            return WithResult(deleteResult);
        }

        #endregion

        #region Helper methods

        private async Task<Models.PageModel> GetItemModelAsync(IPage page)
        {
            return new Models.PageModel
            {
                Id = page.Id,
                CreatedDate = page.CreatedDate,
                Title = page.Header,
                Status = page.IsPublished ? Models.PageStatus.Published : Models.PageStatus.Draft,
                Url = await pageLinkGenerator.GetPathAsync(page)
            };
        }
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