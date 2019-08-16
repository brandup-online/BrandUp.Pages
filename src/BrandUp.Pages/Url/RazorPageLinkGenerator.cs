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

        public Task<string> GetPathAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var urlPath = GetPagePath(page);

            var httpContext = httpContextAccessor.HttpContext;
            var pageUrl = linkGenerator.GetPathByPage(httpContext, RazorPagePath, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        public Task<string> GetUriAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var urlPath = GetPagePath(page);

            var httpContext = httpContextAccessor.HttpContext;
            var pageUrl = linkGenerator.GetUriByPage(httpContext, RazorPagePath, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        private string GetPagePath(IPage page)
        {
            if (pageUrlHelper.IsDefaultUrlPath(page.UrlPath))
                return string.Empty;
            else
                return pageUrlHelper.NormalizeUrlPath(page.UrlPath);
        }

        public Task<string> GetPathAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default)
        {
            if (pageEditSession == null)
                throw new ArgumentNullException(nameof(pageEditSession));

            var url = linkGenerator.GetPathByPage(httpContextAccessor.HttpContext, RazorPagePath, null, new { editId = pageEditSession.Id.ToString().ToLower() });

            return Task.FromResult(url);
        }

        public Task<string> GetUriAsync(IPageEdit pageEditSession, CancellationToken cancellationToken = default)
        {
            if (pageEditSession == null)
                throw new ArgumentNullException(nameof(pageEditSession));

            var url = linkGenerator.GetUriByPage(httpContextAccessor.HttpContext, RazorPagePath, null, new { editId = pageEditSession.Id.ToString().ToLower() });

            return Task.FromResult(url);
        }

        public Task<string> GetPathAsync(string pagePath, CancellationToken cancellationToken = default)
        {
            if (pagePath == null)
                pagePath = string.Empty;

            var urlPath = NormalizePagePath(pagePath);
            var pageUrl = linkGenerator.GetPathByPage(httpContextAccessor.HttpContext, RazorPagePath, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        public Task<string> GetUriAsync(string pagePath, CancellationToken cancellationToken = default)
        {
            if (pagePath == null)
                pagePath = string.Empty;

            var urlPath = NormalizePagePath(pagePath);
            var pageUrl = linkGenerator.GetUriByPage(httpContextAccessor.HttpContext, RazorPagePath, null, new { url = urlPath });

            return Task.FromResult(pageUrl);
        }

        private string NormalizePagePath(string pagePath)
        {
            if (pageUrlHelper.IsDefaultUrlPath(pagePath))
                return string.Empty;
            else
                return pageUrlHelper.NormalizeUrlPath(pagePath);
        }
    }
}