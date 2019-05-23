using BrandUp.Pages.Files;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Url
{
    public class FileUrlGenerator : IFileUrlGenerator
    {
        private readonly LinkGenerator linkGenerator;

        public FileUrlGenerator(LinkGenerator linkGenerator)
        {
            this.linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
        }

        public Task<string> GetImageUrlAsync(ImageValue image, int width = 0, int height = 0, CancellationToken cancellationToken = default)
        {
            if (!image.HasValue)
                throw new ArgumentException();

            string url;
            switch (image.ValueType)
            {
                case ImageValueType.Id:
                    url = linkGenerator.GetPathByAction("Image", "File", new { fileId = image.Value, width, height });
                    break;
                case ImageValueType.Url:
                    url = image.Value;
                    if (!url.StartsWith("http"))
                    {
                        if (url.StartsWith("~"))
                            url = url.Substring(1);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return Task.FromResult(url);
        }
    }
}