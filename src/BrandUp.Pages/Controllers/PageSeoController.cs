using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Models;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/page/seo", Name = "BrandUp.Pages.Page.Seo"), Administration.Administration]
    public class PageSeoController : FormController<PageSeoForm, PageSeoValues, PageModel>
    {
        private readonly IPageService pageService;
        private readonly IPageLinkGenerator pageLinkGenerator;
        private IPage page;

        public PageSeoController(IPageService pageService, IPageLinkGenerator pageLinkGenerator)
        {
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
        }

        #region FormController methods

        protected override async Task OnInitializeAsync()
        {
            if (!Request.Query.TryGetValue("pageId", out string pageIdValue))
            {
                AddErrors("Not valid id.");
                return;
            }

            if (!Guid.TryParse(pageIdValue, out Guid pageId))
            {
                AddErrors("Not valid id.");
                return;
            }

            page = await pageService.FindPageByIdAsync(pageId);
            if (page == null)
            {
                AddErrors("Not found page collection.");
                return;
            }
        }

        protected override async Task OnBuildFormAsync(PageSeoForm formModel)
        {
            formModel.Page = await GetPageModelAsync(page);

            var seoOptions = await pageService.GetPageSeoOptionsAsync(page, HttpContext.RequestAborted);

            formModel.Values.Title = seoOptions.Title;
            formModel.Values.Description = seoOptions.Description;
            if (seoOptions.Keywords != null)
                formModel.Values.Keywords = string.Join(", ", seoOptions.Keywords);
        }

        protected override Task OnChangeValueAsync(string field, PageSeoValues values)
        {
            return Task.CompletedTask;
        }

        protected override async Task<PageModel> OnCommitAsync(PageSeoValues values)
        {
            var keywords = new List<string>();
            if (!string.IsNullOrEmpty(values.Keywords))
            {
                foreach (var word in values.Keywords.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    var n = word.Trim();
                    if (string.IsNullOrEmpty(n))
                        continue;

                    keywords.Add(word);
                }
            }

            await pageService.UpdatePageSeoOptionsAsync(page, new PageSeoOptions
            {
                Title = values.Title,
                Description = values.Description,
                Keywords = keywords.Count > 0 ? keywords.ToArray() : null
            }, HttpContext.RequestAborted);

            return await GetPageModelAsync(page);
        }

        #endregion

        #region Helper methods

        private async Task<PageModel> GetPageModelAsync(IPage page)
        {
            return new PageModel
            {
                Id = page.Id,
                CreatedDate = page.CreatedDate,
                Title = page.Header,
                Status = page.IsPublished ? PageStatus.Published : PageStatus.Draft,
                Url = await pageLinkGenerator.GetUrlAsync(page)
            };
        }

        #endregion
    }
}