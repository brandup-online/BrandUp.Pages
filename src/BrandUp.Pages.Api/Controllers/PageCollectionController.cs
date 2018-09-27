using BrandUp.Pages.Api.DataModels;
using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Api
{
    [ApiController]
    [Route("api/collection")]
    public class PageCollectionController : Controller
    {
        private readonly IPageCollectionService pageCollectionService;

        public PageCollectionController(IPageCollectionService pageCollectionService)
        {
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
        }

        public async Task<ActionResult> Get([FromQuery]Guid? pageId)
        {
            var result = new List<DataModels.PageCollectionModel>();

            var collections = await pageCollectionService.GetCollectionsAsync(pageId);
            foreach (var collection in collections)
                result.Add(collection.CreateDataModel());

            return Ok(result);
        }

        [Route("default"), HttpGet]
        public ActionResult Get()
        {
            return Ok(new DataModels.PageCollectionModel { PageSort = PageSortMode.FirstOld });
        }

        [Route("{id}"), HttpGet]
        public async Task<ActionResult> Get(Guid id)
        {
            var collection = await pageCollectionService.FindCollectiondByIdAsync(id);
            if (collection == null)
                return NotFound();

            return Ok(collection.CreateDataModel());
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]UiModels.NewPageCollectionModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var collection = await pageCollectionService.CreateCollectionAsync(model.Title, model.PageTypeName, model.PageSort, model.PageId);

            return Created(string.Empty, collection.CreateDataModel());
        }

        [Route("{id}"), HttpPut]
        public async Task<ActionResult> Put(Guid id, [FromBody]UiModels.UpdatePageCollectionModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedCollection = await pageCollectionService.UpdateCollectionAsync(id, model.Title, model.PageSort);

            return Ok(updatedCollection);
        }

        [Route("{id}"), HttpDelete]
        public async Task<ActionResult> Delete(Guid id)
        {
            var collection = await pageCollectionService.FindCollectiondByIdAsync(id);
            if (collection == null)
                return BadRequest();

            await pageCollectionService.DeleteCollectionAsync(collection);

            return Ok();
        }
    }
}