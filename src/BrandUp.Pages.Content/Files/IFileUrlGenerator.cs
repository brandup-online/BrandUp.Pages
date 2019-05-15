using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content.Files
{
    public interface IFileUrlGenerator
    {
        Task<string> GetImageUrlAsync(ImageValue image, CancellationToken cancellationToken = default);
    }
}