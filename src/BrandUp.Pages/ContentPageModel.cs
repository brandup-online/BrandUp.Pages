using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Services;
using BrandUp.Pages.Url;
using BrandUp.Website;
using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    public sealed class ContentPageModel : AppPageModel
    {
        IPage page;
        IContentEdit editSession;
        PageSeoOptions pageSeo;

        #region Properties

        [FromQuery(Name = "editId"), ClientProperty]
        public Guid? EditId { get; set; }
        public IPageService PageService { get; private set; }
        public IPage PageEntry => page;
        public PageMetadataProvider PageMetadata { get; private set; }
        public object PageContent { get; private set; }
        public ContentContext ContentContext { get; private set; }
        [ClientProperty]
        public Guid Id => page.Id;
        [ClientProperty]
        public Models.PageStatus Status { get; private set; }
        [ClientProperty]
        public Guid? ParentPageId { get; private set; }

        #endregion

        #region AppPageModel members

        public override string Title => !string.IsNullOrEmpty(pageSeo.Title) ? pageSeo.Title : PageMetadata.GetPageHeader(PageContent);
        public override string Description => pageSeo.Description;
        public override string Keywords => pageSeo.Keywords != null ? string.Join(",", pageSeo.Keywords) : null;
        public override string ScriptName => "content";
        protected override async Task OnPageRequestAsync(PageRequestContext context)
        {
            PageService = HttpContext.RequestServices.GetRequiredService<IPageService>();

            if (EditId.HasValue)
            {
                var pageEditingService = HttpContext.RequestServices.GetRequiredService<IContentEditService>();
                editSession = await pageEditingService.FindEditByIdAsync(EditId.Value, CancellationToken);
                if (editSession == null)
                {
                    context.Result = NotFound();
                    return;
                }

                page = await PageService.FindPageByIdAsync(editSession.PageId, CancellationToken);
                if (page == null)
                {
                    context.Result = NotFound();
                    return;
                }

                var accessProvider = HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
                if (!await accessProvider.CheckAccessAsync(CancellationToken) || await accessProvider.GetUserIdAsync(CancellationToken) != editSession.UserId)
                {
                    var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();

                    context.Result = RedirectPermanent(await pageLinkGenerator.GetPathAsync(page, CancellationToken));
                    return;
                }
            }
            else
            {
                var routeData = RouteData;

                var pagePath = string.Empty;
                if (routeData.Values.TryGetValue("url", out object urlValue) && urlValue != null)
                    pagePath = (string)urlValue;

                var url = await PageService.FindUrlByPathAsync(WebsiteContext.Website.Id, pagePath, CancellationToken);
                if (url == null)
                {
                    context.Result = NotFound();
                    return;
                }

                if (url.PageId.HasValue)
                {
                    page = await PageService.FindPageByIdAsync(url.PageId.Value, CancellationToken);
                    if (page == null)
                    {
                        context.Result = NotFound();
                        return;
                    }

                    if (!page.IsPublished)
                    {
                        var accessProvider = HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
                        if (!await accessProvider.CheckAccessAsync(CancellationToken))
                        {
                            context.Result = NotFound();
                            return;
                        }
                    }
                }
                else
                {
                    var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
                    var redirectUrl = await pageLinkGenerator.GetPathAsync(url.Redirect.Path, CancellationToken);

                    if (url.Redirect.IsPermament)
                        context.Result = RedirectPermanent(redirectUrl);
                    else
                        context.Result = Redirect(redirectUrl);
                    return;
                }
            }

            PageMetadata = await PageService.GetPageTypeAsync(page, CancellationToken);

            pageSeo = await PageService.GetPageSeoOptionsAsync(page, CancellationToken);
            var pageContentKey = await PageService.GetContentKeyAsync(page.Id, CancellationToken);

            if (editSession != null)
            {
                var pageEditingService = HttpContext.RequestServices.GetRequiredService<IContentEditService>();
                PageContent = await pageEditingService.GetContentAsync(editSession, CancellationToken);
            }
            else
            {
                var contentService = HttpContext.RequestServices.GetRequiredService<ContentService>();
                PageContent = await contentService.GetContentAsync(page.WebsiteId, pageContentKey, CancellationToken);
            }

            if (PageContent == null)
                throw new InvalidOperationException($"Not set page content.");

            ContentContext = new ContentContext(pageContentKey, PageContent, HttpContext.RequestServices, editSession != null);

            Status = page.IsPublished ? Models.PageStatus.Published : Models.PageStatus.Draft;
            ParentPageId = await PageService.GetParentPageIdAsync(page, CancellationToken);
        }

        #endregion
    }
}