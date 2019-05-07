using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Url
{
    public class RazorPageLinkGenerator : IPageLinkGenerator
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly RazorContentPageOptions options;
        private readonly IPageUrlHelper pageUrlHelper;

        public RazorPageLinkGenerator(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IOptions<RazorContentPageOptions> options, IPageUrlHelper pageUrlHelper)
        {
            this.linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.options = options.Value ?? throw new ArgumentNullException(nameof(options.Value));
            this.pageUrlHelper = pageUrlHelper ?? throw new ArgumentNullException(nameof(pageUrlHelper));
        }

        public Task<string> GetPageUrl(IPage page)
        {
            string pageUrl;
            var httpContext = httpContextAccessor.HttpContext;

            if (page.UrlPath == null)
                pageUrl = linkGenerator.GetUriByPage(httpContext, options.RazorPagePath, null, new { pageId = page.Id.ToString().ToLower() });
            else
            {
                string urlPath;
                if (pageUrlHelper.IsDefaultUrlPath(page.UrlPath))
                    urlPath = string.Empty;
                else
                    urlPath = pageUrlHelper.NormalizeUrlPath(page.UrlPath);

                pageUrl = linkGenerator.GetUriByPage(httpContext, options.RazorPagePath, null, new { url = urlPath });
            }

            return Task.FromResult(pageUrl);
        }
    }
}