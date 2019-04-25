using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LandingWebSite.Pages
{
    public class IndexModel : AppPageModel
    {
        private readonly IPageService pageService;

        public IndexModel(IPageService pageService)
        {
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        #region AppPageModel members

        public override string Title => null;
        public override string Description => null;
        public override string Keywords => null;

        #endregion

        public async Task<IActionResult> OnGetAsync()
        {
            var routeData = RouteData;

            var pagePath = string.Empty;
            if (routeData.Values.TryGetValue("url", out object urlValue))
                pagePath = (string)urlValue;
            pagePath = pagePath.Trim(new char[] { '/' });

            var page = await pageService.FindPageByPathAsync(pagePath);
            if (page == null)
                return NotFound();

            return Page();
        }
    }
}