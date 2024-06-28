using BrandUp.Pages.Services;
using BrandUp.Website;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    [ApiController, Filters.Administration]
    public class PageCollectionController(PageCollectionService pageCollectionService, PageService pageService, Url.IPageLinkGenerator pageLinkGenerator, IWebsiteContext websiteContext) : ControllerBase
    {
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
            var result = new List<Models.PageCollectionModel>();

            IEnumerable<IPageCollection> collections;
            if (pageId.HasValue)
            {
                var page = await pageService.FindPageByIdAsync(pageId.Value);
                if (page == null)
                    return BadRequest();
                collections = await pageCollectionService.ListCollectionsAsync(page);
            }
            else
            {
                collections = await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id);
            }

            foreach (var pageCollection in collections)
                result.Add(await GetItemModelAsync(pageCollection));

            return Ok(result);
        }

        [HttpGet, Route("brandup.pages/collection/search", Name = "BrandUp.Pages.Collection.Search")]
        public async Task<IActionResult> SearchAsync([FromQuery] string pageType, [FromQuery] string title = null)
        {
            if (pageType == null)
                return BadRequest();

            var result = new List<Models.PageCollectionModel>();

            var collections = await pageCollectionService.FindCollectionsAsync(websiteContext.Website.Id, pageType, title, true);
            foreach (var pageCollection in collections)
                result.Add(await GetItemModelAsync(pageCollection));

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

        private async Task<Models.PageCollectionModel> GetItemModelAsync(IPageCollection pageCollection)
        {
            string pageUrl = "/";
            if (pageCollection.PageId.HasValue)
            {
                IPage page = await pageService.FindPageByIdAsync(pageCollection.PageId.Value);
                pageUrl = await pageLinkGenerator.GetPathAsync(page);
            }

            return new Models.PageCollectionModel
            {
                Id = pageCollection.Id,
                CreatedDate = pageCollection.CreatedDate,
                PageId = pageCollection.PageId,
                Title = pageCollection.Title,
                PageType = pageCollection.PageTypeName,
                Sort = pageCollection.SortMode,
                CustomSorting = pageCollection.CustomSorting,
                PageUrl = pageUrl
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