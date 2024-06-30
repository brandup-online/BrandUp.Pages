using BrandUp.Pages.Features;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Services;
using BrandUp.Pages.Url;
using BrandUp.Website;
using BrandUp.Website.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Pages
{
    public sealed class ContentPageModel : AppPageModel
    {
        IPage page;
        PageSeoOptions pageSeo;

        #region Properties

        public IPage PageEntry => page;
        public PageMetadataProvider PageMetadata { get; private set; }
        [ClientProperty]
        public Guid Id => page.Id;
        [ClientProperty]
        public Models.PageStatus Status { get; private set; }
        [ClientProperty]
        public Guid? ParentPageId { get; private set; }
        public string ContentKey { get; private set; }
        public ContentContext ContentContext { get; private set; }

        #endregion

        #region AppPageModel members

        public override string Title => pageSeo.Title;//!string.IsNullOrEmpty(pageSeo.Title) ? pageSeo.Title : PageMetadata.GetPageHeader(ContentContext.Content);
        public override string Description => pageSeo.Description;
        public override string Keywords => pageSeo.Keywords != null ? string.Join(",", pageSeo.Keywords) : null;
        public override string ScriptName => "content";
        protected override async Task OnPageRequestAsync(PageRequestContext context)
        {
            var httpContext = HttpContext;
            var pageService = httpContext.RequestServices.GetRequiredService<PageService>();

            #region Find page by url

            var pagePath = string.Empty;
            if (RouteData.Values.TryGetValue("url", out object urlValue) && urlValue != null)
                pagePath = (string)urlValue;

            var url = await pageService.FindUrlByPathAsync(WebsiteContext.Website.Id, pagePath, CancellationToken);
            if (url == null)
            {
                context.Result = NotFound();
                return;
            }

            if (url.PageId.HasValue)
            {
                page = await pageService.FindPageByIdAsync(url.PageId.Value, CancellationToken);
                if (page == null)
                {
                    context.Result = NotFound();
                    return;
                }

                if (!page.IsPublished)
                {
                    var accessProvider = httpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
                    if (!await accessProvider.CheckAccessAsync(CancellationToken))
                    {
                        context.Result = NotFound();
                        return;
                    }
                }
            }
            else
            {
                var pageLinkGenerator = httpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
                var redirectUrl = await pageLinkGenerator.GetPathAsync(url.Redirect.Path, CancellationToken);

                if (url.Redirect.IsPermament)
                    context.Result = RedirectPermanent(redirectUrl);
                else
                    context.Result = Redirect(redirectUrl);
                return;
            }

            HttpContext.Features.Set(new ContentPageFeature(page));

            PageMetadata = await pageService.GetPageTypeAsync(page, CancellationToken);
            Status = page.IsPublished ? Models.PageStatus.Published : Models.PageStatus.Draft;
            ParentPageId = await pageService.GetParentPageIdAsync(page, CancellationToken);
            pageSeo = await pageService.GetPageSeoOptionsAsync(page, CancellationToken);

            #endregion

            #region Create content context

            ContentKey = await pageService.GetContentKeyAsync(Id, CancellationToken);

            #endregion
        }

        #endregion
    }
}