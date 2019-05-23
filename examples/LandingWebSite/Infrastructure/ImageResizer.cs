using BrandUp.Pages.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace LandingWebSite.Infrastructure
{
    public class ImageResizer : IImageResizer
    {
        public void Resize(Stream imageStream, int width, int height, Stream output)
        {
            using (Image<Rgba32> image = Image.Load(imageStream))
            {
                image.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Size = new SixLabors.Primitives.Size(width, height) }));
                image.SaveAsJpeg(output, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder() { Quality = 65, Subsample = SixLabors.ImageSharp.Formats.Jpeg.JpegSubsample.Ratio444 });
            }
        }
    }
}