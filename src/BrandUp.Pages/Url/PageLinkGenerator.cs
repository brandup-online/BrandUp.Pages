using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace BrandUp.Pages.Url
{
    public class PageLinkGenerator : IPageLinkGenerator
    {
        readonly LinkGenerator linkGenerator;
        readonly IHttpContextAccessor httpContextAccessor;
        readonly IPageUrlHelper pageUrlHelper;
        readonly RootPageOptions contentPageOptions;

        public PageLinkGenerator(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IPageUrlHelper pageUrlHelper, IOptions<RootPageOptions> contentPageOptions)
        {
            this.linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.pageUrlHelper = pageUrlHelper ?? throw new ArgumentNullException(nameof(pageUrlHelper));
            this.contentPageOptions = contentPageOptions?.Value ?? throw new ArgumentNullException(nameof(contentPageOptions));
        }

        #region IPageLinkGenerator members

        public Task<string> GetPathAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var urlPath = GetPagePath(page);

            var httpContext = httpContextAccessor.HttpContext;
            var pageUrl = linkGenerator.GetPathByPage(httpContext, contentPageOptions.ContentPageName, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        public Task<string> GetUrlAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var urlPath = GetPagePath(page);

            var httpContext = httpContextAccessor.HttpContext;
            var pageUrl = linkGenerator.GetUriByPage(httpContext, contentPageOptions.ContentPageName, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        public Task<string> GetPathAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default)
        {
            if (pageEditSession == null)
                throw new ArgumentNullException(nameof(pageEditSession));

            var url = linkGenerator.GetPathByPage(httpContextAccessor.HttpContext, contentPageOptions.ContentPageName, null, new { editId = pageEditSession.Id.ToString().ToLower() });

            return Task.FromResult(url);
        }

        public Task<string> GetUrlAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default)
        {
            if (pageEditSession == null)
                throw new ArgumentNullException(nameof(pageEditSession));

            var url = linkGenerator.GetUriByPage(httpContextAccessor.HttpContext, contentPageOptions.ContentPageName, null, new { editId = pageEditSession.Id.ToString().ToLower() });

            return Task.FromResult(url);
        }

        public Task<string> GetPathAsync(string pagePath, CancellationToken cancellationToken = default)
        {
            pagePath ??= string.Empty;

            var urlPath = NormalizePagePath(pagePath);
            var pageUrl = linkGenerator.GetPathByPage(httpContextAccessor.HttpContext, contentPageOptions.ContentPageName, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        public Task<string> GetUrlAsync(string pagePath, CancellationToken cancellationToken = default)
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