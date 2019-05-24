using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Files;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    public class ImageController : FieldController<IImageField>
    {
        readonly FileService fileService;
        readonly Files.IFileUrlGenerator fileUrlGenerator;

        public ImageController(FileService fileService, Files.IFileUrlGenerator fileUrlGenerator)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            this.fileUrlGenerator = fileUrlGenerator ?? throw new ArgumentNullException(nameof(fileUrlGenerator));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromQuery]string fileName, [FromQuery]string width = null, [FromQuery]string height = null)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest();

            var contentType = Request.ContentType;
            if (!contentType.StartsWith("image"))
                return BadRequest();

            var file = await fileService.UploadFileAsync(Page, fileName, contentType, Request.Body);

            var modelValue = new ImageValue(file.Id);
            Field.SetModelValue(ContentContext.Content, modelValue);

            await SaveChangesAsync();

            if (width != null && height != null)
            {
                var fielUrl = await fileUrlGenerator.GetImageUrlAsync(modelValue, int.Parse(width), int.Parse(height));
                return Ok(fielUrl);
            }

            return await FormValueAsync();
        }
    }
}