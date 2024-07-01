using BrandUp.Pages.Models;
using BrandUp.Pages.Services;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/page/seo", Name = "BrandUp.Pages.Page.Seo"), Filters.Administration]
    public class PageSeoController(PageService pageService, IPageLinkGenerator pageLinkGenerator) : FormController<PageSeoForm, PageSeoValues, PageModel>
    {
        IPage page;

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
            if (seoOptions.Keywords != null && seoOptions.Keywords.Length > 0)
                formModel.Values.Keywords = seoOptions.Keywords;
        }

        protected override Task OnChangeValueAsync(string field, PageSeoValues values)
        {
            return Task.CompletedTask;
        }

        protected override async Task<PageModel> OnCommitAsync(PageSeoValues values)
        {
            var keywords = new List<string>();
            if (values.Keywords != null)
            {
                foreach (var word in values.Keywords)
                {
                    var normalizeWord = word.Trim();
                    if (string.IsNullOrEmpty(normalizeWord))
                        continue;

                    keywords.Add(normalizeWord);
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
                Url = await pageLinkGenerator.GetPathAsync(page)
            };
        }

        #endregion
    }
}