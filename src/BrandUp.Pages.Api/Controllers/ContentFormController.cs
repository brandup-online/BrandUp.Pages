using BrandUp.Pages.Api.DataModels;
using BrandUp.Pages.Api.FormModels;
using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Api.Controllers
{
    [ApiController]
    [Route("api/content/form")]
    public class ContentFormController : Controller
    {
        private readonly IPageEditingService editingService;
        private readonly IContentMetadataManager contentMetadataManager;

        public ContentFormController(IContentMetadataManager contentMetadataManager, IPageEditingService editingService)
        {
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
            this.editingService = editingService ?? throw new ArgumentNullException(nameof(editingService));
        }

        [HttpGet("{path?}")]
        public async Task<ActionResult> GetModelAsync([FromQuery(Name = "esid")]Guid editSessionId, string path)
        {
            var editSession = await editingService.FindEditSessionById(editSessionId);
            if (editSession == null)
                return BadRequest();

            var pageContentModel = await editingService.GetContentAsync(editSession);
            var pageContentExplorer = ContentExplorer.Create(contentMetadataManager, pageContentModel);

            var contentExplorer = pageContentExplorer.Navigate(path ?? string.Empty);
            if (contentExplorer == null)
                return NotFound();

            var formModel = CreateFormModel(contentExplorer);

            return Ok(formModel);
        }
        private ContentFormModel CreateFormModel(ContentExplorer explorer)
        {
            var model = new ContentFormModel
            {
                Metadata = explorer.Metadata.CreateDataModel(),
                Fields = new List<ContentFormFieldModel>()
            };

            foreach (var field in explorer.Metadata.Fields)
            {
                var modelValue = field.GetModelValue(explorer.Content);
                var formValue = field.GetFormValue(modelValue);
                var formOptions = field.GetFormOptions();

                model.Fields.Add(new ContentFormFieldModel
                {
                    FieldType = field.GetType().Name,
                    Name = field.Name,
                    Title = field.Title,
                    Required = field.IsRequired,
                    Options = formOptions,
                    Value = formValue
                });
            }

            return model;
        }
    }
}