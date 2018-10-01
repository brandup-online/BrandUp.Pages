using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Api.Controllers
{
    [ApiController]
    [Route("api/content/image")]
    public class ImageFieldController : Controller
    {
        private static readonly string[] AlloweredImageTypes = new string[] { "jpeg", "png" };

        private readonly IPageEditingService editingService;
        private readonly IContentMetadataManager contentMetadataManager;
        private readonly IFileService fileService;

        public ImageFieldController(IContentMetadataManager contentMetadataManager, IPageEditingService editingService, IFileService fileService)
        {
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
            this.editingService = editingService ?? throw new ArgumentNullException(nameof(editingService));
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        [HttpPut("{path?}")]
        public async Task<ActionResult> UploadAsync([FromQuery(Name = "esid")]Guid editSessionId, string path, [FromQuery]string fieldName, [FromQuery]string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest();

            var contentType = Request.ContentType;
            if (!contentType.StartsWith("image"))
                return BadRequest();
            var fileType = contentType.Substring(contentType.IndexOf("/") + 1);

            var editSession = await editingService.FindEditSessionById(editSessionId);
            if (editSession == null)
                return BadRequest();

            var pageContentModel = await editingService.GetContentAsync(editSession);
            var pageContentExplorer = ContentExplorer.Create(contentMetadataManager, pageContentModel);

            var contentExplorer = pageContentExplorer.Navigate(path ?? string.Empty);

            if (!contentExplorer.Metadata.TryGetField(fieldName, out ImageField field))
                return BadRequest();

            var file = await fileService.UploadFileAsync(fileName, Request.Body);

            var modelValue = new ImageValue(file.Id);
            field.SetModelValue(contentExplorer.Content, new ImageValue(file.Id));

            await editingService.SetContentAsync(editSession, pageContentExplorer.Content);

            return Accepted(field.GetFormValue(modelValue));
        }
    }
}