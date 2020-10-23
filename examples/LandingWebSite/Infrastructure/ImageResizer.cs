using BrandUp.Pages.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace LandingWebSite.Infrastructure
{
    public class ImageResizer : IImageResizer
    {
        public Task Resize(Stream imageStream, int width, int height, Stream output)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(imageStream))
            {
                var resizeOptions = new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Size = new Size(width, height),
                    //CenterCoordinates = new List<float> { 5000, 5000 }
                };

                image.Mutate(x => x.Resize(resizeOptions));
                image.SaveAsJpeg(output, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder() { Quality = 65, Subsample = SixLabors.ImageSharp.Formats.Jpeg.JpegSubsample.Ratio444 });
            }

            return Task.CompletedTask;
        }
    }
}