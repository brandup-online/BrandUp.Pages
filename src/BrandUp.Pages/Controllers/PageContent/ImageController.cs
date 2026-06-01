using System.Net;
using System.Net.Sockets;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Files;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
	public class ImageController : FieldController<IImageField>
	{
		// Переиспользуемый клиент, чтобы не исчерпывать сокеты (per-request new HttpClient — антипаттерн).
		// AutomaticDecompression выкл., редиректы запрещены: иначе SSRF-валидацию можно обойти редиректом на приватный адрес.
		static readonly HttpClient httpClient = new(new SocketsHttpHandler
		{
			AllowAutoRedirect = false,
			ConnectTimeout = TimeSpan.FromSeconds(10)
		})
		{
			Timeout = TimeSpan.FromSeconds(30),
			MaxResponseContentBufferSize = 20 * 1024 * 1024 // 20 МБ потолок на загружаемое изображение
		};

		readonly FileService fileService;
		readonly Files.IFileUrlGenerator fileUrlGenerator;

		public ImageController(FileService fileService, Files.IFileUrlGenerator fileUrlGenerator)
		{
			this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
			this.fileUrlGenerator = fileUrlGenerator ?? throw new ArgumentNullException(nameof(fileUrlGenerator));
		}

		[HttpPost]
		public async Task<IActionResult> PostAsync([FromQuery] string fileName, [FromQuery] string? width = null, [FromQuery] string? height = null)
		{
			if (string.IsNullOrEmpty(fileName))
				return BadRequest();

			var contentType = Request.ContentType;
			if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("image"))
				return BadRequest();

			var file = await fileService.UploadFileAsync(Page, fileName, contentType, Request.Body);

			var modelValue = new ImageValue(file.Id);
			Field.SetModelValue(ContentContext.Content, modelValue);

			await SaveChangesAsync();

			if (TryParseSize(width, height, out var w, out var h))
			{
				var fielUrl = await fileUrlGenerator.GetImageUrlAsync(modelValue, w, h);
				return Ok(fielUrl);
			}

			return await FormValueAsync();
		}

		[HttpPost("url")]
		public async Task<IActionResult> UrlAsync([FromQuery] string url, [FromQuery] string? width = null, [FromQuery] string? height = null)
		{
			if (string.IsNullOrEmpty(url))
				return BadRequest();

			if (!await IsSafeRemoteUrlAsync(url))
				return BadRequest();

			using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
			if (response.StatusCode != HttpStatusCode.OK)
				return BadRequest();

			var contentType = response.Content.Headers.ContentType?.MediaType;
			if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("image"))
				return BadRequest();

			using (var contentStream = await response.Content.ReadAsStreamAsync())
			{
				var file = await fileService.UploadFileAsync(Page, url, contentType, contentStream);

				var modelValue = new ImageValue(file.Id);
				Field.SetModelValue(ContentContext.Content, modelValue);

				await SaveChangesAsync();

				if (TryParseSize(width, height, out var w, out var h))
				{
					var fielUrl = await fileUrlGenerator.GetImageUrlAsync(modelValue, w, h);
					return Ok(fielUrl);
				}
			}

			return await FormValueAsync();
		}

		static bool TryParseSize(string? width, string? height, out int w, out int h)
		{
			w = h = 0;
			return width != null && height != null
				&& int.TryParse(width, out w) && int.TryParse(height, out h);
		}

		/// <summary>
		/// Защита от SSRF: разрешаем только http/https и публичные адреса.
		/// Блокируем loopback, private, link-local (включая cloud metadata 169.254.169.254) и др. не-маршрутизируемые диапазоны.
		/// </summary>
		static async Task<bool> IsSafeRemoteUrlAsync(string url)
		{
			if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
				return false;

			if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
				return false;

			IPAddress[] addresses;
			try
			{
				addresses = IPAddress.TryParse(uri.Host, out var literal)
					? [literal]
					: await Dns.GetHostAddressesAsync(uri.Host);
			}
			catch
			{
				return false;
			}

			if (addresses.Length == 0)
				return false;

			foreach (var address in addresses)
			{
				if (IsPrivateOrReserved(address))
					return false;
			}

			return true;
		}

		static bool IsPrivateOrReserved(IPAddress address)
		{
			if (IPAddress.IsLoopback(address))
				return true;

			if (address.AddressFamily == AddressFamily.InterNetwork)
			{
				var b = address.GetAddressBytes();
				// 10.0.0.0/8
				if (b[0] == 10) return true;
				// 172.16.0.0/12
				if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return true;
				// 192.168.0.0/16
				if (b[0] == 192 && b[1] == 168) return true;
				// 169.254.0.0/16 (link-local, cloud metadata)
				if (b[0] == 169 && b[1] == 254) return true;
				// 127.0.0.0/8
				if (b[0] == 127) return true;
				// 0.0.0.0/8
				if (b[0] == 0) return true;
				// 100.64.0.0/10 (CGNAT)
				if (b[0] == 100 && b[1] >= 64 && b[1] <= 127) return true;
			}
			else if (address.AddressFamily == AddressFamily.InterNetworkV6)
			{
				if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6UniqueLocal)
					return true;
				// ::1 покрывается IsLoopback; проверим IPv4-mapped адреса
				if (address.IsIPv4MappedToIPv6 && IsPrivateOrReserved(address.MapToIPv4()))
					return true;
			}

			return false;
		}
	}
}