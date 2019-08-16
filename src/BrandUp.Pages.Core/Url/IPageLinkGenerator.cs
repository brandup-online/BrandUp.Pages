using BrandUp.Pages.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Url
{
    public interface IPageLinkGenerator
    {
        Task<string> GetPathAsync(IPage page, CancellationToken cancellationToken = default);
        Task<string> GetUriAsync(IPage page, CancellationToken cancellationToken = default);
        Task<string> GetPathAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default);
        Task<string> GetUriAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default);
        Task<string> GetPathAsync(string pagePath, CancellationToken cancellationToken = default);
        Task<string> GetUriAsync(string pagePath, CancellationToken cancellationToken = default);
    }
}