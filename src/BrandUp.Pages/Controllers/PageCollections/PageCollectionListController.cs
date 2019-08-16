using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/collection/list", Name = "BrandUp.Pages.Collection.List"), Filters.Administration]
    public class PageCollectionListController : ListController<PageCollectionListModel, PageCollectionModel, IPageCollection, Guid>
    {
        readonly IPageService pageService;
        readonly IPageCollectionService pageCollectionService;
        readonly Url.IPageLinkGenerator pageLinkGenerator;
        private IPage page;

        public PageCollectionListController(IPageService pageService, IPageCollectionService pageCollectionService, Url.IPageLinkGenerator pageLinkGenerator)
        {
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
        }

        #region ListController members

        protected override async Task OnInitializeAsync()
        {
            if (Request.Query.TryGetValue("pageId", out string pageIdValue))
            {
                if (!Guid.TryParse(pageIdValue, out Guid pageId))
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
        }

        protected override async Task OnBuildListAsync(PageCollectionListModel listModel)
        {
            listModel.Parents = new List<string>();

            if (page != null)
            {
                listModel.Parents.Add(page.Header);

                IPage currentPage = page;
                while (currentPage != null)
                {
                    var parentPageId = await pageService.GetParentPageIdAsync(currentPage);
                    if (!parentPageId.HasValue)
                        break;

                    currentPage = await pageService.FindPageByIdAsync(parentPageId.Value);
                    listModel.Parents.Add(currentPage.Header);
                }

                listModel.Parents.Reverse();
            }
        }

        protected override Guid ParseId(string value)
        {
            return Guid.Parse(value);
        }

        protected override Task<IEnumerable<IPageCollection>> OnGetItemsAsync(int offset, int limit)
        {
            return pageCollectionService.GetCollectionsAsync(page?.Id);
        }

        protected override Task<IPageCollection> OnGetItemAsync(Guid id)
        {
            return pageCollectionService.FindCollectiondByIdAsync(id);
        }

        protected override async Task<PageCollectionModel> OnGetItemModelAsync(IPageCollection item)
        {
            string pageUrl = "/";
            if (item.PageId.HasValue)
            {
                IPage page = await pageService.FindPageByIdAsync(item.PageId.Value);
                pageUrl = await pageLinkGenerator.GetPathAsync(page);
            }

            return new PageCollectionModel
            {
                Id = item.Id,
                CreatedDate = item.CreatedDate,
                PageId = item.PageId,
                Title = item.Title,
                PageType = item.PageTypeName,
                Sort = item.SortMode,
                CustomSorting = item.CustomSorting,
                PageUrl = pageUrl
            };
        }

        #endregion
    }
}