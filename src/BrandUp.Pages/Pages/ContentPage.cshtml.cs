using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public sealed class ContentPageModel : AppPageModel
    {
        private IPage page;
        private IPageEditSession editSession;

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

        public override string Title => PageMetadata.GetPageTitle(PageContent);
        public override string ScriptName => "content";

        #endregion

        #region AppPageModel members

        protected override async Task OnInitializeAsync(PageHandlerExecutingContext context)
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

                var administrationManager = HttpContext.RequestServices.GetRequiredService<Administration.IAdministrationManager>();

                if (!await administrationManager.CheckAsync() || await administrationManager.GetUserIdAsync() != editSession.ContentManagerId)
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
                pagePath = pagePath.Trim(new char[] { '/' });

                page = await PageService.FindPageByPathAsync(pagePath);
                if (page == null)
                {
                    context.Result = NotFound();
                    return;
                }
            }

            PageMetadata = await PageService.GetPageTypeAsync(page);

            if (editSession != null)
            {
                var pageEditingService = HttpContext.RequestServices.GetRequiredService<IPageEditingService>();
                PageContent = await pageEditingService.GetContentAsync(editSession);
            }
            else
                PageContent = await PageService.GetPageContentAsync(page);
            if (PageContent == null)
                throw new InvalidOperationException();

            ContentContext = new ContentContext(page, PageContent, HttpContext.RequestServices);

            var isPublished = await PageService.IsPublishedAsync(page);
            Status = isPublished ? Models.PageStatus.Published : Models.PageStatus.Draft;
            ParentPageId = await PageService.GetParentPageIdAsync(page);
        }

        #endregion

        #region Handler methods

        public async Task<IActionResult> OnPostBeginEditAsync([FromServices]IPageEditingService pageEditingService, [FromServices]IPageLinkGenerator pageLinkGenerator)
        {
            if (editSession != null)
                return BadRequest();

            editSession = await pageEditingService.BeginEditAsync(page);

            return new OkObjectResult(await pageLinkGenerator.GetUrlAsync(editSession));
        }

        public async Task<IActionResult> OnGetFormModelAsync([FromQuery]string contentPath)
        {
            if (editSession == null)
                return BadRequest();
            if (contentPath == null)
                contentPath = string.Empty;

            var contentContext = ContentContext.Navigate(contentPath);
            if (contentContext == null)
                return BadRequest();

            var formModel = new Models.PageContentForm
            {
                Path = contentContext.Explorer.Path
            };

            foreach (var field in contentContext.Explorer.Metadata.Fields)
            {
                formModel.Fields.Add(new Models.ContentFieldModel
                {
                    Type = field.Type,
                    Name = field.Name,
                    Title = field.Title,
                    Options = field.GetFormOptions(contentContext.Services)
                });

                var modelValue = field.GetModelValue(contentContext.Content);
                var formValue = await field.GetFormValueAsync(modelValue, contentContext.Services);

                formModel.Values.Add(field.Name, formValue);
            }

            return new OkObjectResult(formModel);
        }

        public async Task<IActionResult> OnPostCommitEditAsync([FromServices]IPageEditingService pageEditingService, [FromServices]IPageLinkGenerator pageLinkGenerator)
        {
            if (editSession == null)
                return BadRequest();

            await pageEditingService.CommitEditSessionAsync(editSession);
            editSession = null;

            return new OkObjectResult(await pageLinkGenerator.GetUrlAsync(page));
        }

        public async Task<IActionResult> OnPostDiscardEditAsync([FromServices]IPageEditingService pageEditingService, [FromServices]IPageLinkGenerator pageLinkGenerator)
        {
            if (editSession == null)
                return BadRequest();

            await pageEditingService.DiscardEditSession(editSession);
            editSession = null;

            return new OkObjectResult(await pageLinkGenerator.GetUrlAsync(page));
        }

        #endregion
    }
}