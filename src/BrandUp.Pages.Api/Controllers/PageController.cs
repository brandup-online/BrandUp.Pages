using BrandUp.Pages.Api.DataModels;
using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Api
{
    [ApiController, Route("api/page")]
    public class PageController : Controller
    {
        private readonly IPageService pageService;
        private readonly IPageCollectionService pageCollectionService;

        public PageController(IPageService pageService, IPageCollectionService pageCollectionService)
        {
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
        }

        [HttpGet]
        public async Task<ActionResult> ListAsync([FromQuery]Guid collectionId, [FromQuery]int skip = 0, [FromQuery]int limit = 5)
        {
            var collection = await pageCollectionService.FindCollectiondByIdAsync(collectionId);
            if (collection == null)
                return BadRequest();

            var result = new List<DataModels.PageModel>();
            var pages = await pageService.GetPagesAsync(collection, new PagePaginationOptions(skip, limit));
            foreach (var page in pages)
                result.Add(await CreatePageModelAsync(page));

            return Ok(result);
        }

        [Route("{pageId}"), HttpGet]
        public async Task<ActionResult> Get(Guid pageId)
        {
            var page = await pageService.FindPageByIdAsync(pageId);
            if (page == null)
                return NotFound();

            var pageModel = await CreatePageModelAsync(page);

            return Ok(pageModel);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromQuery]Guid collectionId)
        {
            var collection = await pageCollectionService.FindCollectiondByIdAsync(collectionId);
            if (collection == null)
                return BadRequest();

            var page = await pageService.CreatePageAsync(collection);

            return Ok(await CreatePageModelAsync(page));
        }

        [Route("{pageId}/publish"), HttpPut]
        public async Task<ActionResult> Publish(Guid pageId, [FromBody]UiModels.PublishPageModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var page = await pageService.FindPageByIdAsync(pageId);
            if (page == null)
                return NotFound();

            await pageService.PublishPageAsync(page, model.UrlPathName);

            return Ok();
        }

        [Route("{pageId}"), HttpDelete]
        public async Task<ActionResult> Delete(Guid pageId)
        {
            var page = await pageService.FindPageByIdAsync(pageId);
            if (page == null)
                return BadRequest();

            await pageService.DeletePageAsync(page);

            return Ok();
        }

        private async Task<PageModel> CreatePageModelAsync(IPage page)
        {
            var pageType = await pageService.GetPageTypeAsync(page);
            var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(page.OwnCollectionId);
            var pageContent = await pageService.GetPageContentAsync(page);

            return new PageModel
            {
                Id = page.Id,
                Type = pageType.CreateDataModel(),
                Collection = pageCollection.CreateDataModel(),
                Status = page.UrlPath == null ? "Draft" : "Published",
                Name = pageType.GetPageName(pageContent),
                Url = page.UrlPath
            };
        }
    }
}