using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Models;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/page/{id}/publish", Name = "BrandUp.Pages.Page.Publish")]
    public class PagePublishController : FormController<PagePublishForm, PagePublishValues, PagePublishResult>
    {
        private readonly IPageService pageService;
        private readonly IPageLinkGenerator pageLinkGenerator;
        private readonly IPageUrlPathGenerator pageUrlPathGenerator;
        private IPage page;

        public PagePublishController(IPageService pageService, IPageLinkGenerator pageLinkGenerator, IPageUrlPathGenerator pageUrlPathGenerator)
        {
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
            this.pageUrlPathGenerator = pageUrlPathGenerator ?? throw new ArgumentNullException(nameof(pageUrlPathGenerator));
        }

        #region Action methods

        protected override async Task OnInitializeAsync()
        {
            if (!RouteData.Values.TryGetValue("id", out object pageIdValue))
            {
                AddErrors("Not valid id.");
                return;
            }

            if (!Guid.TryParse(pageIdValue.ToString(), out Guid pageId))
            {
                AddErrors("Not valid id.");
                return;
            }

            page = await pageService.FindPageByIdAsync(pageId);
            if (page == null)
            {
                AddErrors("Not found page.");
                return;
            }
        }

        protected override async Task OnBuildFormAsync(PagePublishForm formModel)
        {
            formModel.Page = await GetItemModelAsync(page);

            formModel.Values.Title = page.Title;
            formModel.Values.UrlPath = await pageUrlPathGenerator.GenerateAsync(page);
        }

        protected override Task OnChangeValueAsync(string field, PagePublishValues values)
        {
            return Task.CompletedTask;
        }

        protected override async Task<PagePublishResult> OnCommitAsync(PagePublishValues values)
        {
            var publishResult = await pageService.PublishPageAsync(page, values.UrlPath);
            if (!publishResult.Succeeded)
            {
                AddErrors(publishResult);
                return null;
            }

            return new PagePublishResult
            {
                Url = await pageLinkGenerator.GetPageUrl(page)
            };
        }

        #endregion

        #region Helper methods

        private async Task<PageModel> GetItemModelAsync(IPage page)
        {
            var isPublished = await pageService.IsPublishedAsync(page);

            return new PageModel
            {
                Id = page.Id,
                CreatedDate = page.CreatedDate,
                Title = page.Title,
                Status = isPublished ? PageStatus.Published : PageStatus.Draft,
                Url = await pageLinkGenerator.GetPageUrl(page)
            };
        }

        #endregion
    }
}