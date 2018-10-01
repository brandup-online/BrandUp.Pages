using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Api
{
    [ApiController, Route("api/page")]
    public class PageEditController : Controller
    {
        private readonly IPageService pageService;
        private readonly IPageEditingService pageEditingService;

        public PageEditController(IPageService pageService, IPageEditingService pageEditingService)
        {
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.pageEditingService = pageEditingService ?? throw new ArgumentNullException(nameof(pageEditingService));
        }

        [Route("{pageId}/edit"), HttpPost]
        public async Task<ActionResult> BeginEdit(Guid pageId)
        {
            var page = await pageService.FindPageByIdAsync(pageId);
            if (page == null)
                return NotFound();

            var editSession = await pageEditingService.BeginEditAsync(page);

            var editUrl = ""; //Url.WebSitePage(page, new { es = editSession.Id });

            return Content(editUrl);
        }

        [Route("{pageId}/edit/{sessionId}"), HttpPut]
        public async Task<ActionResult> Commit(Guid sessionId)
        {
            var editSession = await pageEditingService.FindEditSessionById(sessionId);
            if (editSession == null)
                return BadRequest();

            await pageEditingService.CommitEditSessionAsync(editSession);

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            if (page == null)
                return BadRequest();

            return Content(page.UrlPath);
        }

        [Route("{pageId}/edit/{sessionId}"), HttpDelete]
        public async Task<ActionResult> Discard(Guid sessionId)
        {
            var editSession = await pageEditingService.FindEditSessionById(sessionId);
            if (editSession == null)
                return BadRequest();

            await pageEditingService.DiscardEditSession(editSession);

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            if (page == null)
                return BadRequest();

            return Content(page.UrlPath);
        }
    }
}