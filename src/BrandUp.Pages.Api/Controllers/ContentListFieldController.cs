using BrandUp.Pages.Api.DataModels;
using BrandUp.Pages.Api.FormModels;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Api.Controllers
{
    [ApiController]
    [Route("api/content/list")]
    public class ContentListFieldController : Controller
    {
        private readonly IPageEditingService editingService;
        private readonly IContentMetadataManager contentMetadataManager;
        private readonly IContentViewManager contentViewManager;

        public ContentListFieldController(IContentMetadataManager contentMetadataManager, IContentViewManager contentViewManager, IPageEditingService editingService)
        {
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
            this.contentViewManager = contentViewManager ?? throw new ArgumentNullException(nameof(contentViewManager));
            this.editingService = editingService ?? throw new ArgumentNullException(nameof(editingService));
        }

        [HttpGet("{path?}")]
        public async Task<ActionResult> GetAsync([FromQuery(Name = "esid")]Guid editSessionId, string path, [FromQuery]string fieldName)
        {
            var editSession = await editingService.FindEditSessionById(editSessionId);
            if (editSession == null)
                return BadRequest();

            var pageContentModel = await editingService.GetContentAsync(editSession);
            var pageContentExplorer = ContentExplorer.Create(contentMetadataManager, contentViewManager, pageContentModel);

            var contentExplorer = pageContentExplorer.Navigate(path ?? string.Empty);

            if (!contentExplorer.Metadata.TryGetField(fieldName, out ContentListField field))
                return BadRequest();

            var result = new ContentListModel
            {
                Items = new List<ContentListItemModel>()
            };

            if (field.GetModelValue(contentExplorer.Content) is IList list)
            {
                foreach (var item in list)
                {
                    var itemMetadata = contentMetadataManager.GetMetadata(item.GetType());

                    result.Items.Add(new ContentListItemModel
                    {
                        Metadata = itemMetadata.CreateDataModel()
                    });
                }
            }

            return Ok(result);
        }

        [HttpPost("{path?}")]
        public async Task<ActionResult> AddAsync([FromQuery(Name = "esid")]Guid editSessionId, string path, [FromQuery]string fieldName, [FromQuery]string contentType)
        {
            var editSession = await editingService.FindEditSessionById(editSessionId);
            if (editSession == null)
                return BadRequest();
            if (!contentMetadataManager.TryGetMetadata(contentType, out ContentMetadataProvider contentMetadata))
                return BadRequest();

            var pageContentModel = await editingService.GetContentAsync(editSession);
            var pageContentExplorer = ContentExplorer.Create(contentMetadataManager, contentViewManager, pageContentModel);

            var contentExplorer = pageContentExplorer.Navigate(path ?? string.Empty);

            if (!contentExplorer.Metadata.TryGetField(fieldName, out ContentListField field))
                return BadRequest();

            var list = field.GetModelValue(contentExplorer.Content) as IList;
            if (list == null)
            {
                list = field.CreateValue();
                field.SetModelValue(contentExplorer.Content, list);
            }

            var newContentModel = contentMetadata.CreateModelInstance();
            list.Add(newContentModel);

            await editingService.SetContentAsync(editSession, pageContentExplorer.Content);

            return Ok(field.GetFormValue(list));
        }

        [HttpDelete("{path?}")]
        public async Task<ActionResult> DeleteAsync([FromQuery(Name = "esid")]Guid editSessionId, string path, [FromQuery]string fieldName, [FromQuery]int index)
        {
            var editSession = await editingService.FindEditSessionById(editSessionId);
            if (editSession == null)
                return BadRequest();

            var pageContentModel = await editingService.GetContentAsync(editSession);
            var pageContentExplorer = ContentExplorer.Create(contentMetadataManager, contentViewManager, pageContentModel);

            var contentExplorer = pageContentExplorer.Navigate(path ?? string.Empty);

            if (!contentExplorer.Metadata.TryGetField(fieldName, out ContentListField field))
                return BadRequest();

            var list = field.GetModelValue(contentExplorer.Content) as IList;

            list.RemoveAt(index);

            await editingService.SetContentAsync(editSession, pageContentExplorer.Content);

            return Ok(field.GetFormValue(list));
        }
    }
}