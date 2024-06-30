using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Models;
using BrandUp.Pages.Services;
using BrandUp.Website;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/page/list", Name = "BrandUp.Pages.Page.List"), Filters.Administration]
    public class PageListController(PageService pageService, PageCollectionService pageCollectionService, Url.IPageLinkGenerator pageLinkGenerator, IWebsiteContext websiteContext, ContentService contentService, PageMetadataManager pageMetadataManager) : ListController<PageListModel, PageModel, IPage, Guid>
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

            IEnumerable<IPageCollection> collections;
            if (page != null)
                collections = await pageCollectionService.ListCollectionsAsync(page);
            else
                collections = await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id);

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
            string pageHeader = null;
            var content = await contentService.FindContentAsync(websiteContext.Website.Id, item, HttpContext.RequestAborted);
            if (content != null)
            {
                var pageContent = await contentService.GetContentAsync(content.CommitId, HttpContext.RequestAborted);
                var pageMetadata = pageMetadataManager.GetMetadata(pageContent.GetType());
                pageHeader = pageMetadata.GetPageHeader(pageContent);
            }
            else
                pageHeader = "New page";

            return new PageModel
            {
                Id = item.Id,
                CreatedDate = item.CreatedDate,
                Title = pageHeader,
                Status = item.IsPublished ? PageStatus.Published : PageStatus.Draft,
                Url = await pageLinkGenerator.GetPathAsync(item)
            };
        }

        protected override async Task OnSortAsync(IPage sourceItem, IPage destItem, ListItemSortPosition position)
        {
            switch (position)
            {
                case ListItemSortPosition.Before:
                    await pageService.UpPagePositionAsync(sourceItem, destItem);
                    break;
                case ListItemSortPosition.After:
                    await pageService.DownPagePositionAsync(sourceItem, destItem);
                    break;
            }
        }

        #endregion

        async Task<PageCollectionModel> GetPageCollectionModelAsync(IPageCollection pageCollection)
        {
            string pageUrl = "/";
            if (pageCollection.PageId.HasValue)
            {
                IPage page = await pageService.FindPageByIdAsync(pageCollection.PageId.Value);
                pageUrl = await pageLinkGenerator.GetPathAsync(page);
            }

            return new PageCollectionModel
            {
                Id = pageCollection.Id,
                CreatedDate = pageCollection.CreatedDate,
                PageId = pageCollection.PageId,
                Title = pageCollection.Title,
                PageType = pageCollection.PageTypeName,
                Sort = pageCollection.SortMode,
                CustomSorting = pageCollection.CustomSorting,
                PageUrl = pageUrl
            };
        }
        async Task<PagePathModel> GetPathModelAsync(IPage page)
        {
            return new PagePathModel
            {
                Id = page.Id,
                Header = page.Header,
                Url = await pageLinkGenerator.GetPathAsync(page),
                Type = page.TypeName,
            };
        }
    }
}