using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Content.Files;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    public class ImageController : FieldController<IImageField>
    {
        private readonly FileService fileService;

        public ImageController(FileService fileService)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromQuery]string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest();

            var contentType = Request.ContentType;
            if (!contentType.StartsWith("image"))
                return BadRequest();
            var fileType = contentType.Substring(contentType.IndexOf("/") + 1);

            var file = await fileService.UploadFileAsync(Page, fileName, contentType, Request.Body);

            //if (Field.TryGetModelValue(ContentContext.Content, out ImageValue curValue) && curValue.ValueType == ImageValueType.Id)
            //{
            //    await fileService.DeleteFileAsync(curValue);
            //}

            var modelValue = new ImageValue(file.Id);
            Field.SetModelValue(ContentContext.Content, modelValue);

            await SaveChangesAsync();

            return await FormValueAsync();
        }
    }
}