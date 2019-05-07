using BrandUp.Pages.Interfaces;
using System.Threading.Tasks;

namespace BrandUp.Pages.Url
{
    public interface IPageLinkGenerator
    {
        Task<string> GetPageUrl(IPage page);
    }
}