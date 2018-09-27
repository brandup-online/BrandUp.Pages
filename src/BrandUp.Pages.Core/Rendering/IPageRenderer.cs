using System.IO;
using System.Threading.Tasks;

namespace BrandUp.Pages.Rendering
{
    public interface IPageRenderer
    {
        Task RenderPageAsync(PageContext context, Stream output);
    }
}