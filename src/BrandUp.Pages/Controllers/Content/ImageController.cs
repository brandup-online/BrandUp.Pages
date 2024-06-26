using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Files;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    public class ImageController(FileService fileService, IFileUrlGenerator fileUrlGenerator) : FieldController<IImageField>
    {
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromQuery] string fileName, [FromQuery] string width = null, [FromQuery] string height = null)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest();

            var contentType = Request.ContentType;
            if (!contentType.StartsWith("image"))
                return BadRequest();

            var file = await fileService.UploadFileAsync(ContentEdit.WebsiteId, ContentEdit.ContentKey, fileName, contentType, Request.Body);

            var modelValue = new ImageValue(file.Id);
            Field.SetModelValue(ContentContext.Content, modelValue);

            await SaveChangesAsync();

            if (width != null && height != null)
            {
                var fielUrl = await fileUrlGenerator.GetImageUrlAsync(modelValue, int.Parse(width), int.Parse(height));
                return Ok(fielUrl);
            }

            return await FormValueResultAsync();
        }

        [HttpPost("url")]
        public async Task<IActionResult> UrlAsync([FromQuery] string url, [FromQuery] string width = null, [FromQuery] string height = null)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest();

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return BadRequest();

                var contentType = response.Content.Headers.ContentType.MediaType;
                if (!contentType.StartsWith("image"))
                    return BadRequest();

                var file = await fileService.UploadFileAsync(ContentEdit.WebsiteId, ContentEdit.ContentKey, url, contentType, await response.Content.ReadAsStreamAsync());

                var modelValue = new ImageValue(file.Id);
                Field.SetModelValue(ContentContext.Content, modelValue);

                await SaveChangesAsync();

                if (width != null && height != null)
                {
                    var fielUrl = await fileUrlGenerator.GetImageUrlAsync(modelValue, int.Parse(width), int.Parse(height));
                    return Ok(fielUrl);
                }
            }

            return await FormValueResultAsync();
        }
    }
}