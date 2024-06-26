using BrandUp.Pages.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LandingWebSite.Infrastructure
{
    public class ImageResizer : IImageResizer
    {
        public async Task ResizeAsync(Stream imageStream, int width, int height, Stream output, CancellationToken cancellationToken)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(imageStream);

            var resizeOptions = new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(width, height),
                //CenterCoordinates = new List<float> { 5000, 5000 }
            };

            image.Mutate(x => x.Resize(resizeOptions));
            await image.SaveAsJpegAsync(output, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder() { Quality = 65 }, cancellationToken);
        }
    }
}