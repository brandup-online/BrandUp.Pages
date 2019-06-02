using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/page/list", Name = "BrandUp.Pages.Page.List"), Administration.Administration]
    public class PageListController : ListController<PageListModel, PageModel, IPage, Guid>
    {
        readonly IPageService pageService;
        readonly IPageCollectionService pageCollectionService;
        readonly Url.IPageLinkGenerator pageLinkGenerator;
        private IPage page;

        public PageListController(IPageService pageService, IPageCollectionService pageCollectionService, Url.IPageLinkGenerator pageLinkGenerator)
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

        protected override async Task OnBuildListAsync(PageListModel listModel)
        {
            listModel.Parents = new List<PagePathModel>();
            listModel.Collections = new List<PageCollectionModel>();

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

            var collections = await pageCollectionService.GetCollectionsAsync(page?.Id);
            foreach (var collection in collections)
                listModel.Collections.Add(await GetPageCollectionModelAsync(collection));
        }

        protected override Guid ParseId(string value)
        {
            return Guid.Parse(value);
        }

        protected override Task<IEnumerable<IPage>> OnGetItemsAsync(int offset, int limit)
        {
            if (!Request.Query.TryGetValue("collectionId", out string collectionIdValue))
            {
                AddErrors("Not valid id.");
                return Task.FromResult<IEnumerable<IPage>>(null);
            }

            if (!Guid.TryParse(collectionIdValue, out Guid collectionId))
            {
                AddErrors("Not valid id.");
                return Task.FromResult<IEnumerable<IPage>>(null);
            }

            return pageService.GetPagesAsync(new GetPagesOptions(collectionId) { IncludeDrafts = true, Pagination = new PagePaginationOptions(offset, limit) });
        }

        protected override Task<IPage> OnGetItemAsync(Guid id)
        {
            return pageService.FindPageByIdAsync(id);
        }

        protected override async Task<PageModel> OnGetItemModelAsync(IPage item)
        {
            return new PageModel
            {
                Id = item.Id,
                CreatedDate = item.CreatedDate,
                Title = item.Header,
                Status = item.IsPublished ? PageStatus.Published : PageStatus.Draft,
                Url = await pageLinkGenerator.GetUrlAsync(item)
            };
        }

        #endregion

        private async Task<PageCollectionModel> GetPageCollectionModelAsync(IPageCollection pageCollection)
        {
            string pageUrl = "/";
            if (pageCollection.PageId.HasValue)
            {
                IPage page = await pageService.FindPageByIdAsync(pageCollection.PageId.Value);
                pageUrl = await pageLinkGenerator.GetUrlAsync(page);
            }

            return new PageCollectionModel
            {
                Id = pageCollection.Id,
                CreatedDate = pageCollection.CreatedDate,
                PageId = pageCollection.PageId,
                Title = pageCollection.Title,
                PageType = pageCollection.PageTypeName,
                Sort = pageCollection.SortMode,
                PageUrl = pageUrl
            };
        }
        private async Task<PagePathModel> GetPathModelAsync(IPage page)
        {
            return new PagePathModel
            {
                Id = page.Id,
                Header = page.Header,
                Url = await pageLinkGenerator.GetUrlAsync(page)
            };
        }
    }
}