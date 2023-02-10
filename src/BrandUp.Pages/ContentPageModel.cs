using BrandUp.Pages.Metadata;
using BrandUp.Pages.Url;
using BrandUp.Website;
using BrandUp.Website.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    /// <summary>
    /// Базовая модель страницы с контентом <see cref="TContent"/>. На странице использующей эту модель можно разместить 
    /// контент только с этим типом или типами наследующими его.
    /// </summary>
    /// <typeparam name="TContent">Тип контента страницы.</typeparam>
    public class ContentPageModel<TContent> : AppPageModel, IContentPageModel
        where TContent : class
    {
        private IPage page;
        private IPageEdit editSession;
        private PageSeoOptions pageSeo;

        #region Properties

        public IPageService PageService { get; private set; }
        public IPage PageEntry => page;
        public PageMetadataProvider PageMetadata { get; private set; }
        public TContent PageContent { get; private set; }
        public ContentContext ContentContext { get; private set; }
        [ClientProperty]
        public Guid Id => page.Id;
        [ClientProperty]
        public Guid? EditId => editSession?.Id;
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

                var accessProvider = HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();

                if (!await accessProvider.CheckAccessAsync() || await accessProvider.GetUserIdAsync() != editSession.UserId)
                {
                    var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();

                    context.Result = RedirectPermanent(await pageLinkGenerator.GetPathAsync(page));
                    return;
                }
            }
            else
            {
                var routeData = RouteData;

                var pagePath = string.Empty;
                if (routeData.Values.TryGetValue("url", out object urlValue) && urlValue != null)
                    pagePath = (string)urlValue;

                var url = await PageService.FindUrlByPathAsync(WebsiteContext.Website.Id, pagePath);
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

                    if (!page.IsPublished)
                    {
                        var accessProvider = HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
                        if (!await accessProvider.CheckAccessAsync())
                        {
                            context.Result = NotFound();
                            return;
                        }
                    }
                }
                else
                {
                    var pageLinkGenerator = HttpContext.RequestServices.GetRequiredService<IPageLinkGenerator>();
                    var redirectUrl = await pageLinkGenerator.GetPathAsync(url.Redirect.Path);

                    if (url.Redirect.IsPermament)
                        context.Result = RedirectPermanent(redirectUrl);
                    else
                        context.Result = Redirect(redirectUrl);

                    return;
                }
            }

            PageMetadata = await PageService.GetPageTypeAsync(page, HttpContext.RequestAborted);

            pageSeo = await PageService.GetPageSeoOptionsAsync(page, HttpContext.RequestAborted);

            object pageContent;
            if (editSession != null)
            {
                var pageContentService = HttpContext.RequestServices.GetRequiredService<IPageContentService>();
                pageContent = await pageContentService.GetContentAsync(editSession, HttpContext.RequestAborted);
            }
            else
                pageContent = await PageService.GetPageContentAsync(page, HttpContext.RequestAborted);
            if (pageContent == null)
                throw new InvalidOperationException("Не найден контент для страницы.");
            if (pageContent is not TContent)
                throw new InvalidOperationException($"Тип контента страницы не наследует тип {typeof(TContent).AssemblyQualifiedName}.");
            PageContent = (TContent)pageContent;

            ContentContext = new ContentContext(page, pageContent, HttpContext.RequestServices, editSession != null);

            Status = page.IsPublished ? Models.PageStatus.Published : Models.PageStatus.Draft;
            ParentPageId = await PageService.GetParentPageIdAsync(page, HttpContext.RequestAborted);
        }

        #endregion
    }

    internal interface IContentPageModel
    {
        ContentContext ContentContext { get; }
    }
}