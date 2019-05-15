using BrandUp.Pages.Content.Files;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("_file")]
    public class FileController : Controller
    {
        private readonly FileService fileService;

        public FileController(FileService fileService)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> Download(Guid fileId)
        {
            var file = await fileService.FindFileByIdAsync(fileId);
            if (file == null)
                return NotFound();

            var fileStream = await fileService.ReadFileAsync(fileId);

            return new FileStreamResult(fileStream, file.ContentType);
        }
    }
}