﻿using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [ApiController]
    public class PageController : ControllerBase
    {
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageService pageService;

        public PageController(IPageCollectionService pageCollectionService, IPageService pageService)
        {
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        #region Action methods

        [HttpGet, Route("brandup.pages/page/{id}", Name = "BrandUp.Pages.Page.Get")]
        public async Task<IActionResult> GetAsync([FromRoute]Guid id)
        {
            var page = await pageService.FindPageByIdAsync(id);
            if (page == null)
                return NotFound();

            var model = GetItemModel(page);

            return Ok(model);
        }

        [HttpGet, Route("brandup.pages/page", Name = "BrandUp.Pages.Page.List")]
        public async Task<IActionResult> ListAsync([FromQuery]Guid collectionId)
        {
            var collection = await pageCollectionService.FindCollectiondByIdAsync(collectionId);
            if (collection == null)
                return BadRequest();

            var result = new List<Models.PageModel>();

            var pages = await pageService.GetPagesAsync(collection, new PagePaginationOptions(0, 20));
            foreach (var page in pages)
            {
                var model = GetItemModel(page);

                result.Add(model);
            }

            return Ok(result);
        }

        [HttpPut, Route("brandup.pages/page", Name = "BrandUp.Pages.Page.Create")]
        public async Task<IActionResult> CreateAsync([FromQuery]Guid collectionId, [FromBody]Models.PageCreateModel requestModel)
        {
            var collection = await pageCollectionService.FindCollectiondByIdAsync(collectionId);
            if (collection == null)
                return BadRequest();

            if (!TryValidateModel(requestModel))
                return BadRequest(ModelState);

            try
            {
                var page = await pageService.CreatePageAsync(collection, requestModel.PageType, requestModel.Title);

                var model = GetItemModel(page);

                return Created(string.Empty, model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return BadRequest(ModelState);
            }
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

        private static Models.PageModel GetItemModel(IPage page)
        {
            return new Models.PageModel
            {
                Id = page.Id,
                CreatedDate = page.CreatedDate,
                Title = page.Title
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