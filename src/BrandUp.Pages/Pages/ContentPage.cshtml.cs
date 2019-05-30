using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public sealed class ContentPageModel : AppPageModel
    {
        private IPage page;
        private IPageEdit editSession;

        #region Properties

        public IPageService PageService { get; private set; }
        public IPage PageEntry => page;
        public PageMetadataProvider PageMetadata { get; private set; }
        public object PageContent { get; private set; }
        public ContentContext ContentContext { get; private set; }
        [ClientModel]
        public Guid Id => page.Id;
        [ClientModel]
        public Guid? EditId => editSession?.Id;
        [ClientModel]
        public Models.PageStatus Status { get; private set; }
        [ClientModel]
        public Guid? ParentPageId { get; private set; }

        #endregion

        #region AppPageModel members

        public override string Title => PageMetadata.GetPageHeader(PageContent);
        public override string ScriptName => "content";
        protected override async Task OnInitializeAsync(PageHandlerExecutingContext context)
        {
            PageService = HttpContext.RequestServices.GetRequiredService<IPageService>();

            if (Request.Query.TryGetValue("editId", out string editIdValue))
            {
                if (!Guid.TryParse(editIdValue, out Guid editId))
                {
                    context.Result = BadRequest();
                    return;
                }

                var pageEditingService = HttpContext.RequestServices.GetRequiredService<IPageContentService>();
                editSession = await pageEditingService.FindEditByIdAsync(editId);
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

                var administrationManager = HttpContext.RequestServices.GetRequiredService<Administration.IAdministrationManager>();

                if (!await administrationManager.CheckAsync() || await administrationManager.GetUserIdAsync() != editSession.UserId)
                {
                    var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();

                    context.Result = RedirectPermanent(await pageLinkGenerator.GetUrlAsync(page));
                    return;
                }
            }
            else
            {
                var routeData = RouteData;

                var pagePath = string.Empty;
                if (routeData.Values.TryGetValue("url", out object urlValue) && urlValue != null)
                    pagePath = (string)urlValue;

                var url = await PageService.FindPageUrlAsync(pagePath);
                if (url == null)
                {
                    context.Result = NotFound();
                    return;
                }

                if (url.PageId.HasValue)
                {
                    page = await PageService.FindPageByIdAsync(url.PageId.Value);
                    if (page == null)
                    {
                        context.Result = NotFound();
                        return;
                    }
                }
                else
                {
                    var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
                    var redirectUrl = await pageLinkGenerator.GetUrlAsync(url.Redirect.Path);

                    if (url.Redirect.IsPermament)
                        context.Result = RedirectPermanent(redirectUrl);
                    else
                        context.Result = Redirect(redirectUrl);
                    return;
                }
            }

            PageMetadata = await PageService.GetPageTypeAsync(page);

            if (editSession != null)
            {
                var pageEditingService = HttpContext.RequestServices.GetRequiredService<IPageContentService>();
                PageContent = await pageEditingService.GetContentAsync(editSession);
            }
            else
                PageContent = await PageService.GetPageContentAsync(page);
            if (PageContent == null)
                throw new InvalidOperationException();

            ContentContext = new ContentContext(page, PageContent, HttpContext.RequestServices, editSession != null);

            Status = page.IsPublished ? Models.PageStatus.Published : Models.PageStatus.Draft;
            ParentPageId = await PageService.GetParentPageIdAsync(page);
        }

        #endregion
    }
}