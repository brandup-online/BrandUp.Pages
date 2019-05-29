using BrandUp.Pages.Files;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    public class FileController : Controller
    {
        readonly FileService fileService;
        readonly IHostingEnvironment hostingEnvironment;
        readonly string imagesTempPath;
        readonly string filesTempPath;

        public FileController(FileService fileService, IHostingEnvironment hostingEnvironment)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            this.hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));

            imagesTempPath = System.IO.Path.Combine(hostingEnvironment.ContentRootPath, "_temp", "images");
            if (!System.IO.Directory.Exists(imagesTempPath))
                System.IO.Directory.CreateDirectory(imagesTempPath);

            filesTempPath = System.IO.Path.Combine(hostingEnvironment.ContentRootPath, "_temp", "files");
            if (!System.IO.Directory.Exists(filesTempPath))
                System.IO.Directory.CreateDirectory(filesTempPath);
        }

        [HttpGet("_file/{fileId}")]
        public async Task<IActionResult> Index(Guid fileId)
        {
            var file = await fileService.FindFileByIdAsync(fileId);
            if (file == null)
                return NotFound();

            var fileTempPath = System.IO.Path.Combine(filesTempPath, $"{fileId}");
            if (!System.IO.File.Exists(fileTempPath))
            {
                using (var fileStream = await fileService.ReadFileAsync(fileId))
                using (var tempFileStream = System.IO.File.OpenWrite(fileTempPath))
                    await fileStream.CopyToAsync(tempFileStream);
            }

            return new FileStreamResult(System.IO.File.OpenRead(fileTempPath), file.ContentType);
        }

        [HttpGet("_image/{fileId}_{width}_{height}.jpg")]
        public async Task<IActionResult> Image(Guid fileId, int width = 0, int height = 0)
        {
            var file = await fileService.FindFileByIdAsync(fileId);
            if (file == null)
                return NotFound();

            if (width == 0 && height == 0)
            {
                width = 1024;
                height = 800;
            }

            var imageResizer = HttpContext.RequestServices.GetService<Images.IImageResizer>();
            if (imageResizer != null)
            {
                var imageTempPath = System.IO.Path.Combine(imagesTempPath, $"{fileId}-{width}-{height}.jpg");
                if (System.IO.File.Exists(imageTempPath))
                    return new FileStreamResult(System.IO.File.OpenRead(imageTempPath), "image/jpeg");

                using (var fileStream = await fileService.ReadFileAsync(fileId))
                using (var tempFileStream = System.IO.File.OpenWrite(imageTempPath))
                    await imageResizer.Resize(fileStream, width, height, tempFileStream);

                return new FileStreamResult(System.IO.File.OpenRead(imageTempPath), "image/jpeg");
            }

            var fileTempPath = System.IO.Path.Combine(filesTempPath, $"{fileId}");
            if (!System.IO.File.Exists(fileTempPath))
            {
                using (var fileStream = await fileService.ReadFileAsync(fileId))
                using (var tempFileStream = System.IO.File.OpenWrite(fileTempPath))
                    await fileStream.CopyToAsync(tempFileStream);
            }

            return new FileStreamResult(System.IO.File.OpenRead(fileTempPath), file.ContentType);
        }
    }
}