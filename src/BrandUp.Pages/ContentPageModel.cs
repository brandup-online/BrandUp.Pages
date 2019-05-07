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

                    context.Result = RedirectPermanent(await pageLinkGenerator.GetPageUrl(page));
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
            PageContent = await PageService.GetPageContentAsync(page);
            if (PageContent == null)
                throw new InvalidOperationException();

            return Page();
        }

        public async Task<IActionResult> OnGetNavigateAsync([FromServices]IPageLinkGenerator pageLinkGenerator)
        {
            var isPublished = await PageService.IsPublishedAsync(page);

            var model = new Models.PageModel
            {
                Id = page.Id,
                CreatedDate = page.CreatedDate,
                Title = page.Title,
                Status = isPublished ? Models.PageStatus.Published : Models.PageStatus.Draft,
                Url = await pageLinkGenerator.GetPageUrl(page)
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