using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Files
{
    public class FakeFileUrlGenerator : IFileUrlGenerator
    {
        public Task<string> GetImageUrlAsync(ImageValue image, int width = 0, int height = 0, CancellationToken cancellationToken = default)
        {
            if (!image.HasValue)
                throw new InvalidOperationException();

            return Task.FromResult(image.Value);
        }
    }
}