using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public class ContentPageModel : PageModel
    {
        private IPage page;
        private IPageEditSession editSession;

        public IPageService PageService { get; private set; }
        public IPage PageEntry => page;
        public object PageContent { get; private set; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            PageService = HttpContext.RequestServices.GetRequiredService<IPageService>();

            if (Request.Query.TryGetValue("pageId", out string pageIdValue))
            {
                if (!Guid.TryParse(pageIdValue, out Guid pageId))
                {
                    context.Result = BadRequest();
                    return;
                }

                page = await PageService.FindPageByIdAsync(pageId);
                if (page == null)
                {
                    context.Result = NotFound();
                    return;
                }

                if (await PageService.IsPublishedAsync(page))
                {
                    var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();

                    context.Result = RedirectPermanent(await pageLinkGenerator.GetUrlAsync(page));
                    return;
                }
            }
            else if (Request.Query.TryGetValue("editId", out string editIdValue))
            {
                if (!Guid.TryParse(editIdValue, out Guid editId))
                {
                    context.Result = BadRequest();
                    return;
                }

                var pageEditingService = HttpContext.RequestServices.GetRequiredService<IPageEditingService>();

                editSession = await pageEditingService.FindEditSessionById(editId);
                if (editSession == null)
                {
                    context.Result = NotFound();
                    return;
                }

                page = await PageService.FindPageByIdAsync(editSession.PageId);
                if (page == null)
                {
                    context.Result = NotFound();
                    return;
                }
            }
            else
            {
                var routeData = RouteData;

                var pagePath = string.Empty;
                if (routeData.Values.TryGetValue("url", out object urlValue))
                    pagePath = (string)urlValue;
                pagePath = pagePath.Trim(new char[] { '/' });

                page = await PageService.FindPageByPathAsync(pagePath);
                if (page == null)
                {
                    context.Result = NotFound();
                    return;
                }
            }

            await base.OnPageHandlerExecutionAsync(context, next);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (editSession != null)
            {
                var pageEditingService = HttpContext.RequestServices.GetRequiredService<IPageEditingService>();
                PageContent = await pageEditingService.GetContentAsync(editSession);
            }
            else
                PageContent = await PageService.GetPageContentAsync(page);
            if (PageContent == null)
                throw new InvalidOperationException();

            return Page();
        }

        public async Task<IActionResult> OnGetNavigateAsync([FromServices]IPageLinkGenerator pageLinkGenerator)
        {
            var isPublished = await PageService.IsPublishedAsync(page);

            var model = new Models.PageNavigationModel
            {
                Id = page.Id,
                ParentPageId = await PageService.GetParentPageIdAsync(page),
                Title = page.Title,
                Status = isPublished ? Models.PageStatus.Published : Models.PageStatus.Draft,
                Url = await pageLinkGenerator.GetUrlAsync(page),
                EditId = editSession?.Id
            };

            return new OkObjectResult(model);
        }
    }

    public enum ContentPageRequestMode
    {
        Page,
        Navigate
    }
}