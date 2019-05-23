using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Files
{
    public interface IFileUrlGenerator
    {
        Task<string> GetImageUrlAsync(ImageValue image, int width = 0, int height = 0, CancellationToken cancellationToken = default);
    }
}