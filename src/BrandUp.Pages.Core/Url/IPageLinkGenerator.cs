using BrandUp.Pages.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Url
{
    public interface IPageLinkGenerator
    {
        Task<string> GetUrlAsync(IPage page, CancellationToken cancellationToken = default);
        Task<string> GetUrlAsync(IPageEditSession pageEditSession, CancellationToken cancellationToken = default);
        Task<string> GetUrlAsync(string pagePath, CancellationToken cancellationToken = default);
    }
}