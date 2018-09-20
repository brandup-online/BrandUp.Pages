using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Mvc.Controllers
{
    public class WebSiteController : Controller
    {
        private readonly IPageService pages;
        private readonly IServiceProvider services;

        public WebSiteController(IPageService pages, IServiceProvider services)
        {
            this.pages = pages ?? throw new ArgumentNullException(nameof(pages));
            this.services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public async Task<IActionResult> Page(string pagepath)
        {
            pagepath = pagepath ?? string.Empty;
            var page = await pages.FindPageByPathAsync(pagepath);
            if (page == null)
                return NotFound();

            return await ViewPageAsync(page);
        }

        public async Task<IActionResult> Draft(Guid pageId)
        {
            var page = await pages.FindPageByIdAsync(pageId);
            if (page == null)
                return NotFound();

            if (page.UrlPath != null)
                return RedirectPermanent(Url.WebSitePage(page));

            return await ViewPageAsync(page);
        }

        private async Task<IActionResult> ViewPageAsync(IPage page)
        {
            var pageContext = new MvcPageContext(HttpContext, page);
            object pageContent;

            if (Request.Query.TryGetValue("es", out StringValues editSessionValues))
            {
                var editSessionId = Guid.Parse(editSessionValues[0]);
                var editSessionService = services.GetRequiredService<IPageEditingService>();

                var editSession = await editSessionService.FindEditSessionById(editSessionId);
                if (editSession == null)
                    return Redirect(Url.WebSitePage(page));

                pageContent = await editSessionService.GetContentAsync(editSession);

                pageContext.SetEditSession(editSession);
            }
            else
                pageContent = await pages.GetPageContentAsync(page);

            ViewData.Add("PageContext", pageContext);

            return View("Page", pageContent);
        }
    }
}