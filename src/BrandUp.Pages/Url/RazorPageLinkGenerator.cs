using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Url
{
    public class RazorPageLinkGenerator : IPageLinkGenerator
    {
        public const string RazorPagePath = "/ContentPage";
        private readonly LinkGenerator linkGenerator;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IPageUrlHelper pageUrlHelper;

        public RazorPageLinkGenerator(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IPageUrlHelper pageUrlHelper)
        {
            this.linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.pageUrlHelper = pageUrlHelper ?? throw new ArgumentNullException(nameof(pageUrlHelper));
        }

        public Task<string> GetUrlAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            string pageUrl;
            string urlPath;
            var httpContext = httpContextAccessor.HttpContext;
            if (pageUrlHelper.IsDefaultUrlPath(page.UrlPath))
                urlPath = string.Empty;
            else
                urlPath = pageUrlHelper.NormalizeUrlPath(page.UrlPath);

            pageUrl = linkGenerator.GetPathByPage(httpContext, RazorPagePath, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }
        public Task<string> GetUrlAsync(IPageEditSession pageEditSession, CancellationToken cancellationToken = default)
        {
            if (pageEditSession == null)
                throw new ArgumentNullException(nameof(pageEditSession));

            var url = linkGenerator.GetPathByPage(httpContextAccessor.HttpContext, RazorPagePath, null, new { editId = pageEditSession.Id.ToString().ToLower() });

            return Task.FromResult(url);
        }
        public Task<string> GetUrlAsync(string pagePath, CancellationToken cancellationToken = default)
        {
            if (pagePath == null)
                pagePath = string.Empty;

            string urlPath;
            if (pageUrlHelper.IsDefaultUrlPath(pagePath))
                urlPath = string.Empty;
            else
                urlPath = pageUrlHelper.NormalizeUrlPath(pagePath);

            var pageUrl = linkGenerator.GetPathByPage(httpContextAccessor.HttpContext, RazorPagePath, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }
    }
}