using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Mvc.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileService fileService;

        public FileController(IFileService fileService)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        [HttpGet]
        public async Task<ActionResult> Get(Guid fileId)
        {
            var file = await fileService.FindFileByIdAsync(fileId);
            if (file == null)
                return NotFound();

            return new FileStreamResult(await fileService.GetFileStreamAsync(fileId), "image/jpeg");
        }
    }
}