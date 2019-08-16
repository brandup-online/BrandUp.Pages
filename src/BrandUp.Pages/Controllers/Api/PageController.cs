using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [ApiController, Filters.Administration]
    public class PageController : ControllerBase
    {
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageService pageService;
        private readonly IPageLinkGenerator pageLinkGenerator;

        public PageController(IPageCollectionService pageCollectionService, IPageService pageService, IPageLinkGenerator pageLinkGenerator)
        {
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
        }

        #region Action methods

        [HttpGet, Route("brandup.pages/page/{id}", Name = "BrandUp.Pages.Page.Get")]
        public async Task<IActionResult> GetAsync([FromRoute]Guid id)
        {
            var page = await pageService.FindPageByIdAsync(id);
            if (page == null)
                return NotFound();

            var model = await GetItemModelAsync(page);

            return Ok(model);
        }

        [HttpGet, Route("brandup.pages/page", Name = "BrandUp.Pages.Page.Items")]
        public async Task<IActionResult> ListAsync([FromQuery]Guid collectionId)
        {
            var collection = await pageCollectionService.FindCollectiondByIdAsync(collectionId);
            if (collection == null)
                return BadRequest();

            var result = new List<Models.PageModel>();

            var pages = await pageService.GetPagesAsync(new GetPagesOptions(collection.Id) { IncludeDrafts = true });
            foreach (var page in pages)
                result.Add(await GetItemModelAsync(page));

            return Ok(result);
        }

        [HttpGet, Route("brandup.pages/page/search", Name = "BrandUp.Pages.Page.Search")]
        public async Task<IActionResult> SearchAsync([FromQuery]string title)
        {
            var result = new List<Models.PageModel>();

            var pages = await pageService.SearchPagesAsync(title, new PagePaginationOptions(0, 20), HttpContext.RequestAborted);
            foreach (var page in pages)
                result.Add(await GetItemModelAsync(page));

            return Ok(result);
        }

        [HttpDelete, Route("brandup.pages/page/{id}", Name = "BrandUp.Pages.Page.Delete")]
        public async Task<IActionResult> DeleteAsync([FromRoute]Guid id)
        {
            var page = await pageService.FindPageByIdAsync(id);
            if (page == null)
                return WithResult(Result.Failed($"Not found page with id \"{id}\"."));

            var deleteResult = await pageService.DeletePageAsync(page);

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

            if (result.Succeeded)
                return Ok();
            else
                return BadRequest(result);
        }

        #endregion
    }
}