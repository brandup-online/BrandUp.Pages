using System.IO;
using System.Threading.Tasks;

namespace BrandUp.Pages.Images
{
    public interface IImageResizer
    {
        Task Resize(Stream imageStream, int width, int height, Stream output);
    }
}