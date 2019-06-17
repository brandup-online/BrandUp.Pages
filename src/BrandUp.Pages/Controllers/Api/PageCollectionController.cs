using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [ApiController, Filters.Administration]
    public class PageCollectionController : ControllerBase
    {
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageService pageService;
        private readonly Url.IPageLinkGenerator pageLinkGenerator;

        public PageCollectionController(IPageCollectionService pageCollectionService, IPageService pageService, Url.IPageLinkGenerator pageLinkGenerator)
        {
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
        }

        #region Action methods

        [HttpGet, Route("brandup.pages/collection/{id}", Name = "BrandUp.Pages.Collection.Get")]
        public async Task<IActionResult> GetAsync([FromRoute]Guid id)
        {
            var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(id);
            if (pageCollection == null)
                return NotFound();

            var model = await GetItemModelAsync(pageCollection);

            return Ok(model);
        }

        [HttpGet, Route("brandup.pages/collection/{id}/pageTypes", Name = "BrandUp.Pages.Collection.GetPageTypes")]
        public async Task<IActionResult> GetPageTypesAsync([FromRoute]Guid id)
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
        public async Task<IActionResult> ListAsync([FromQuery]Guid? pageId)
        {
            var result = new List<Models.PageCollectionModel>();

            var collections = await pageCollectionService.GetCollectionsAsync(pageId);
            foreach (var pageCollection in collections)
                result.Add(await GetItemModelAsync(pageCollection));

            return Ok(result);
        }

        [HttpGet, Route("brandup.pages/collection/search", Name = "BrandUp.Pages.Collection.Search")]
        public async Task<IActionResult> SearchAsync([FromQuery]string pageType, [FromQuery]string title = null)
        {
            if (pageType == null)
                return BadRequest();

            var result = new List<Models.PageCollectionModel>();

            var collections = await pageCollectionService.GetCollectionsAsync(pageType, title, true);
            foreach (var pageCollection in collections)
                result.Add(await GetItemModelAsync(pageCollection));

            return Ok(result);
        }

        [HttpDelete, Route("brandup.pages/collection/{id}", Name = "BrandUp.Pages.Collection.Delete")]
        public async Task<IActionResult> DeleteAsync([FromRoute]Guid id)
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
                pageUrl = await pageLinkGenerator.GetUrlAsync(page);
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

            if (result.Succeeded)
                return Ok();
            else
                return BadRequest(result);
        }

        #endregion
    }
}