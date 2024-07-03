using BrandUp.Pages.Files;
using Microsoft.AspNetCore.Routing;

namespace BrandUp.Pages.Url
{
    public class FileUrlGenerator(LinkGenerator linkGenerator) : IFileUrlGenerator
    {
        public Task<string> GetImageUrlAsync(ImageValue image, int width = 0, int height = 0, CancellationToken cancellationToken = default)
        {
            if (!image.HasValue)
                throw new ArgumentException();

            string url;
            switch (image.ValueType)
            {
                case ImageValueType.Id:
                    if (width > 0 || height > 0)
                        url = linkGenerator.GetPathByAction("Image", "File", new { fileId = image.Value, width, height });
                    else
                        url = linkGenerator.GetPathByAction("Index", "File", new { fileId = image.Value });
                    break;
                case ImageValueType.Url:
                    url = NormalizeUrl(image.Value);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return Task.FromResult(url);
        }

        static string NormalizeUrl(string url)
        {
            if (url.StartsWith("~"))
                return url[1..];

            return url;
        }
    }
}