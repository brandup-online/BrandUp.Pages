using System.IO;

namespace BrandUp.Pages.Images
{
    public interface IImageResizer
    {
        void Resize(Stream imageStream, int width, int height, Stream output);
    }
}