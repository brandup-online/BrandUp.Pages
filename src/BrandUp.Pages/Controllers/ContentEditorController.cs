using BrandUp.Pages.Administration;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/editor"), ApiController, Administration.Administration]
    public class ContentEditorController : ControllerBase
    {
        private readonly ContentEditorManager contentEditorManager;

        public ContentEditorController(ContentEditorManager contentEditorManager)
        {
            this.contentEditorManager = contentEditorManager ?? throw new ArgumentNullException(nameof(contentEditorManager));
        }

        [HttpGet, Route("{id}", Name = "BrandUp.Pages.Editor.Get")]
        public async Task<IActionResult> GetAsync([FromRoute]string id)
        {
            var pageCollection = await contentEditorManager.FindByIdAsync(id);
            if (pageCollection == null)
                return NotFound();

            var model = GetItemModel(pageCollection);

            return Ok(model);
        }

        [HttpGet, Route("", Name = "BrandUp.Pages.Editor.Items")]
        public IActionResult List()
        {
            var result = new List<Models.ContentEditorModel>();

            var collections = contentEditorManager.ContentEditors;
            foreach (var pageCollection in collections)
                result.Add(GetItemModel(pageCollection));

            return Ok(result);
        }

        [HttpDelete, Route("{id}", Name = "BrandUp.Pages.Editor.Delete")]
        public async Task<IActionResult> DeleteAsync([FromRoute]string id)
        {
            var contentEditor = await contentEditorManager.FindByIdAsync(id);
            if (contentEditor == null)
                return WithResult(Result.Failed($"Not found content editor with id \"{id}\"."));

            var deleteResult = await contentEditorManager.DeleteAsync(contentEditor, HttpContext.RequestAborted);

            return WithResult(deleteResult);
        }

        private Models.ContentEditorModel GetItemModel(IContentEditor contentEditor)
        {
            return new Models.ContentEditorModel
            {
                Id = contentEditor.Id,
                CreatedDate = contentEditor.CreatedDate,
                Email = contentEditor.Email
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
    }
}