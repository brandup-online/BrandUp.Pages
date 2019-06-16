using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/collection/{id}/update", Name = "BrandUp.Pages.Collection.Update"), Filters.Administration]
    public class PageCollectionUpdateController : FormController<PageCollectionUpdateForm, PageCollectionUpdateValues, PageCollectionModel>
    {
        #region Fields

        private readonly IPageCollectionService pageCollectionService;
        private IPageCollection pageCollection;

        #endregion

        public PageCollectionUpdateController(IPageCollectionService pageCollectionService)
        {
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
        }

        #region FormController members

        protected override async Task OnInitializeAsync()
        {
            if (!RouteData.Values.TryGetValue("id", out object pageCollectionIdValue))
            {
                AddErrors("Not valid id.");
                return;
            }

            if (!Guid.TryParse(pageCollectionIdValue.ToString(), out Guid pageCollectionId))
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
        protected override Task OnBuildFormAsync(PageCollectionUpdateForm formModel)
        {
            formModel.PageCollection = GetPageCollectionModel(pageCollection);

            formModel.Values.Title = pageCollection.Title;
            formModel.Values.Sort = pageCollection.SortMode;

            formModel.Sorts = new List<ComboBoxItem>
            {
                new ComboBoxItem(PageSortMode.FirstOld.ToString(), "Сначало старые"),
                new ComboBoxItem(PageSortMode.FirstNew.ToString(), "Сначало новые")
            };

            return Task.CompletedTask;
        }
        protected override Task OnChangeValueAsync(string field, PageCollectionUpdateValues values)
        {
            return Task.CompletedTask;
        }
        protected override async Task<PageCollectionModel> OnCommitAsync(PageCollectionUpdateValues values)
        {
            pageCollection.SetTitle(values.Title);
            pageCollection.SetSortModel(values.Sort);

            await pageCollectionService.UpdateCollectionAsync(pageCollection, HttpContext.RequestAborted);

            return GetPageCollectionModel(pageCollection);
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

        #endregion
    }
}