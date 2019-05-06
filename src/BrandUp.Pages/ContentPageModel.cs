using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public class ContentPageModel : PageModel
    {
        private IPage page;

        public IPage PageEntry => page;
        public object PageContent { get; private set; }

        public async Task<IActionResult> OnGetAsync([FromServices]IPageService pageService)
        {
            if (pageService == null)
                throw new ArgumentNullException(nameof(pageService));

            if (Request.Query.TryGetValue("pageId", out string pageIdValue))
            {
                if (!Guid.TryParse(pageIdValue, out Guid pageId))
                    return BadRequest();

                page = await pageService.FindPageByIdAsync(pageId);
                if (page == null)
                    return NotFound();
            }
            else
            {
                var routeData = RouteData;

                var pagePath = string.Empty;
                if (routeData.Values.TryGetValue("url", out object urlValue))
                    pagePath = (string)urlValue;
                pagePath = pagePath.Trim(new char[] { '/' });

                page = await pageService.FindPageByPathAsync(pagePath);
                if (page == null)
                    return NotFound();
            }

            PageContent = await pageService.GetPageContentAsync(page);
            if (PageContent == null)
                throw new InvalidOperationException();

            return Page();
        }
    }
}