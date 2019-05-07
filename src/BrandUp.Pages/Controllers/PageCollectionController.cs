using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [ApiController]
    public class PageCollectionController : ControllerBase
    {
        private readonly IPageCollectionService pageCollectionService;

        public PageCollectionController(IPageCollectionService pageCollectionService)
        {
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
        }

        #region Action methods

        [HttpGet, Route("brandup.pages/collection/{id}", Name = "BrandUp.Pages.Collection.Get")]
        public async Task<IActionResult> GetAsync([FromRoute]Guid id)
        {
            var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(id);
            if (pageCollection == null)
                return NotFound();

            var model = GetItemModel(pageCollection);

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

        [HttpGet, Route("brandup.pages/collection", Name = "BrandUp.Pages.Collection.List")]
        public async Task<IActionResult> ListAsync([FromQuery]Guid? pageId)
        {
            var result = new List<Models.PageCollectionModel>();

            var collections = await pageCollectionService.GetCollectionsAsync(pageId);
            foreach (var pageCollection in collections)
            {
                var model = GetItemModel(pageCollection);

                result.Add(model);
            }

            return Ok(result);
        }

        [HttpPost, Route("brandup.pages/collection/{id}", Name = "BrandUp.Pages.Collection.Update")]
        public async Task<IActionResult> UpdateAsync([FromRoute]Guid id, [FromBody]Models.PageCollectionUpdateModel requestModel)
        {
            if (!TryValidateModel(requestModel))
                return BadRequest(ModelState);

            var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(id);
            if (pageCollection == null)
                return BadRequest();

            try
            {
                pageCollection = await pageCollectionService.UpdateCollectionAsync(pageCollection.Id, requestModel.Title, requestModel.Sort);

                var model = GetItemModel(pageCollection);

                return Ok(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return BadRequest(ModelState);
            }
        }

        [HttpDelete, Route("brandup.pages/collection/{id}", Name = "BrandUp.Pages.Collection.Delete")]
        public async Task<IActionResult> DeleteAsync([FromRoute]Guid id)
        {
            var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(id);
            if (pageCollection == null)
                return WithResult(BrandUp.Pages.Result.Failed($"Not found page collection with id \"{id}\"."));

            var deleteResult = await pageCollectionService.DeleteCollectionAsync(pageCollection);

            return WithResult(deleteResult);
        }

        #endregion

        #region Helper methods

        private static Models.PageCollectionModel GetItemModel(IPageCollection pageCollection)
        {
            return new Models.PageCollectionModel
            {
                Id = pageCollection.Id,
                CreatedDate = pageCollection.CreatedDate,
                PageId = pageCollection.PageId,
                Title = pageCollection.Title,
                PageType = pageCollection.PageTypeName,
                Sort = pageCollection.SortMode
            };
        }
        private IActionResult WithResult(BrandUp.Pages.Result result)
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