﻿using BrandUp.Pages.Models;
using BrandUp.Pages.Services;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/page/create", Name = "BrandUp.Pages.Page.Create"), Filters.Administration]
    public class PageCreateController(PageService pageService, PageCollectionService pageCollectionService, IPageLinkGenerator pageLinkGenerator) : FormController<PageCreateForm, PageCreateValues, PageModel>
    {
        IPageCollection pageCollection;

        #region FormController methods

        protected override async Task OnInitializeAsync()
        {
            if (!Request.Query.TryGetValue("collectionId", out string pageCollectionIdValue))
            {
                AddErrors("Not valid id.");
                return;
            }

            if (!Guid.TryParse(pageCollectionIdValue, out Guid pageCollectionId))
            {
                AddErrors("Not valid id.");
                return;
            }

            pageCollection = await pageCollectionService.FindCollectiondByIdAsync(pageCollectionId);
            if (pageCollection == null)
            {
                AddErrors("Not found page collection.");
                return;
            }
        }

        protected override async Task OnBuildFormAsync(PageCreateForm formModel)
        {
            formModel.PageCollection = GetPageCollectionModel(pageCollection);

            formModel.PageTypes = (await pageCollectionService.GetPageTypesAsync(pageCollection)).Select(it => new ComboBoxItem(it.Name, it.Title)).ToList();
        }

        protected override Task OnChangeValueAsync(string field, PageCreateValues values)
        {
            return Task.CompletedTask;
        }

        protected override async Task<PageModel> OnCommitAsync(PageCreateValues values)
        {
            var page = await pageService.CreatePageAsync(pageCollection, values.PageType, values.Header, HttpContext.RequestAborted);
            return await GetPageModelAsync(page);
        }

        #endregion

        #region Helper methods

        private PageCollectionModel GetPageCollectionModel(IPageCollection pageCollection)
        {
            return new PageCollectionModel
            {
                Id = pageCollection.Id,
                CreatedDate = pageCollection.CreatedDate,
                PageId = pageCollection.PageId,
                Title = pageCollection.Title,
                PageType = pageCollection.PageTypeName,
                Sort = pageCollection.SortMode
            };
        }
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