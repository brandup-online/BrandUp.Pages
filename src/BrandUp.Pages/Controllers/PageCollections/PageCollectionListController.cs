using BrandUp.Pages.Models;
using BrandUp.Pages.Services;
using BrandUp.Website;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/collection/list", Name = "BrandUp.Pages.Collection.List"), Filters.Administration]
    public class PageCollectionListController(PageService pageService, PageCollectionService pageCollectionService, Url.IPageLinkGenerator pageLinkGenerator, IWebsiteContext websiteContext) : ListController<PageCollectionListModel, PageCollectionModel, IPageCollection, Guid>
    {
        IPage page;

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
            listModel.Parents = new List<PagePathModel>();

            if (page != null)
            {
                listModel.Parents.Add(await GetPathModelAsync(page));

                IPage currentPage = page;
                while (currentPage != null)
                {
                    var parentPageId = await pageService.GetParentPageIdAsync(currentPage);
                    if (!parentPageId.HasValue)
                        break;

                    currentPage = await pageService.FindPageByIdAsync(parentPageId.Value);
                    listModel.Parents.Add(await GetPathModelAsync(currentPage));
                }

                listModel.Parents.Reverse();
            }
        }

        private async Task<PagePathModel> GetPathModelAsync(IPage page)
        {
            return new PagePathModel
            {
                Id = page.Id,
                Header = page.Header,
                Url = await pageLinkGenerator.GetPathAsync(page),
                Type = page.TypeName,
            };
        }

        protected override Guid ParseId(string value)
        {
            return Guid.Parse(value);
        }

        protected override Task<IEnumerable<IPageCollection>> OnGetItemsAsync(int offset, int limit)
        {
            if (page != null)
                return pageCollectionService.ListCollectionsAsync(page);
            else
                return pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id);
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