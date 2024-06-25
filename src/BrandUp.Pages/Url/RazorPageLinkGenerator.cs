using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace BrandUp.Pages.Url
{
    public class RazorPageLinkGenerator(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IPageUrlHelper pageUrlHelper, IOptions<ContentPageOptions> contentPageOptions) : IPageLinkGenerator
    {
        readonly LinkGenerator linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
        readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        readonly IPageUrlHelper pageUrlHelper = pageUrlHelper ?? throw new ArgumentNullException(nameof(pageUrlHelper));
        readonly ContentPageOptions contentPageOptions = contentPageOptions?.Value ?? throw new ArgumentNullException(nameof(contentPageOptions.Value));

        #region IPageLinkGenerator members

        public Task<string> GetPathAsync(IPage page, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);

            var urlPath = GetPagePath(page);

            var httpContext = httpContextAccessor.HttpContext;
            var pageUrl = linkGenerator.GetPathByPage(httpContext, contentPageOptions.ContentPageName, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        public Task<string> GetUriAsync(IPage page, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);

            var urlPath = GetPagePath(page);

            var httpContext = httpContextAccessor.HttpContext;
            var pageUrl = linkGenerator.GetUriByPage(httpContext, contentPageOptions.ContentPageName, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        public Task<string> GetPathAsync(string pagePath, CancellationToken cancellationToken = default)
        {
            pagePath ??= string.Empty;

            var urlPath = NormalizePagePath(pagePath);
            var pageUrl = linkGenerator.GetPathByPage(httpContextAccessor.HttpContext, contentPageOptions.ContentPageName, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        public Task<string> GetUriAsync(string pagePath, CancellationToken cancellationToken = default)
        {
            pagePath ??= string.Empty;

            var urlPath = NormalizePagePath(pagePath);
            var pageUrl = linkGenerator.GetUriByPage(httpContextAccessor.HttpContext, contentPageOptions.ContentPageName, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        #endregion

        string GetPagePath(IPage page)
        {
            if (pageUrlHelper.IsDefaultUrlPath(page.UrlPath))
                return string.Empty;
            else
                return pageUrlHelper.NormalizeUrlPath(page.UrlPath);
        }

        string NormalizePagePath(string pagePath)
        {
            if (pageUrlHelper.IsDefaultUrlPath(pagePath))
                return string.Empty;
            else
                return pageUrlHelper.NormalizeUrlPath(pagePath);
        }
    }
}