using BrandUp.Pages.Interfaces;
using System.Threading.Tasks;

namespace BrandUp.Pages.Url
{
    public interface IPageLinkGenerator
    {
        Task<string> GetUrlAsync(IPage page);
        Task<string> GetUrlAsync(IPageEditSession pageEditSession);
        Task<string> GetUrlAsync(string pagePath);
    }
}