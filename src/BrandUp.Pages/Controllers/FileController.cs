using System.Net;
using BrandUp.Pages.Files;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Controllers
{
	public class FileController : Controller
	{
		readonly FileService fileService;
		readonly string imagesTempPath;
		readonly string filesTempPath;

		public FileController(FileService fileService, IWebHostEnvironment hostingEnvironment)
		{
			this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));

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
				await CacheFromSourceAsync(fileId, fileTempPath);

			var cacheFileDate = System.IO.File.GetLastWriteTimeUtc(fileTempPath);
			if (IsNotModified(cacheFileDate))
				return StatusCode((int)HttpStatusCode.NotModified);

			SetETag(cacheFileDate);

			return new FileStreamResult(System.IO.File.OpenRead(fileTempPath), file.ContentType);
		}

		[HttpGet("_image/{fileId}_{width}_{height}.jpg")]
		public async Task<IActionResult> Image(Guid fileId, int width = 0, int height = 0)
		{
			var file = await fileService.FindFileByIdAsync(fileId);
			if (file == null)
				return NotFound();

			// Эндпоинт отдаёт изображение — отказываемся обслуживать не-изображения,
			// чтобы _image не превращался в произвольную выгрузку файлов.
			if (string.IsNullOrEmpty(file.ContentType) || !file.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
				return NotFound();

			if (width == 0 && height == 0)
			{
				width = 1024;
				height = 800;
			}

			DateTime cacheFileDate;
			var imageResizer = HttpContext.RequestServices.GetService<Images.IImageResizer>();
			if (imageResizer != null)
			{
				var imageTempPath = System.IO.Path.Combine(imagesTempPath, $"{fileId}-{width}-{height}.jpg");
				if (!System.IO.File.Exists(imageTempPath))
				{
					await WriteAtomicAsync(imageTempPath, async target =>
					{
						using var fileStream = await fileService.ReadFileAsync(fileId);
						await imageResizer.Resize(fileStream, width, height, target);
					});
				}

				cacheFileDate = System.IO.File.GetLastWriteTimeUtc(imageTempPath);
				if (IsNotModified(cacheFileDate))
					return StatusCode((int)HttpStatusCode.NotModified);

				SetETag(cacheFileDate);

				return new FileStreamResult(System.IO.File.OpenRead(imageTempPath), "image/jpeg");
			}

			var fileExtension = fileService.GetFileExtension(file);

			var fileTempPath = System.IO.Path.Combine(filesTempPath, $"{fileId}{fileExtension}");
			if (!System.IO.File.Exists(fileTempPath))
				await CacheFromSourceAsync(fileId, fileTempPath);

			cacheFileDate = System.IO.File.GetLastWriteTimeUtc(fileTempPath);
			if (IsNotModified(cacheFileDate))
				return StatusCode((int)HttpStatusCode.NotModified);

			SetETag(cacheFileDate);

			return new FileStreamResult(System.IO.File.OpenRead(fileTempPath), file.ContentType);
		}

		async Task CacheFromSourceAsync(Guid fileId, string targetPath)
		{
			await WriteAtomicAsync(targetPath, async target =>
			{
				using var fileStream = await fileService.ReadFileAsync(fileId);
				await fileStream.CopyToAsync(target);
			});
		}

		// Пишем во временный файл с уникальным именем и атомарно переименовываем в целевой.
		// Так в кэш никогда не попадёт частично записанный (повреждённый) файл при сбое/обрыве.
		static async Task WriteAtomicAsync(string targetPath, Func<Stream, Task> write)
		{
			var tempPath = targetPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
			try
			{
				using (var tempFileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
					await write(tempFileStream);

				System.IO.File.Move(tempPath, targetPath, overwrite: true);
			}
			catch
			{
				if (System.IO.File.Exists(tempPath))
				{
					try { System.IO.File.Delete(tempPath); } catch { /* подчищаем по мере возможности */ }
				}
				throw;
			}
		}

		bool IsNotModified(DateTime cacheFileDate)
		{
			var ifNoneMatch = HttpContext.Request.Headers.IfNoneMatch.ToString();
			if (string.IsNullOrEmpty(ifNoneMatch))
				return false;

			var currentETag = cacheFileDate.Ticks.ToString();
			// Клиент возвращает ETag в кавычках (возможно с префиксом W/); сравниваем устойчиво.
			return ifNoneMatch.Trim().Trim('"').Replace("W/", "").Trim('"') == currentETag;
		}

		void SetETag(DateTime cacheFileDate)
		{
			HttpContext.Response.Headers.ETag = "\"" + cacheFileDate.Ticks.ToString() + "\"";
		}
	}
}