using BrandUp.Pages.Metadata;
using BrandUp.Pages.Models;
using BrandUp.Pages.Url;
using BrandUp.Website;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/collection/create", Name = "BrandUp.Pages.Collection.Create"), Filters.Administration]
    public class PageCollectionCreateController : FormController<PageCollectionCreateForm, PageCollectionCreateValues, PageCollectionModel>
    {
        #region Fields

        readonly IPageService pageService;
        readonly IPageCollectionService pageCollectionService;
        readonly IPageLinkGenerator pageLinkGenerator;
        readonly IPageMetadataManager pageMetadataManager;
        readonly IWebsiteContext websiteContext;
        private IPage page;

        #endregion

        public PageCollectionCreateController(
            IPageService pageService,
            IPageCollectionService pageCollectionService,
            IPageLinkGenerator pageLinkGenerator,
            IPageMetadataManager pageMetadataManager,
            IWebsiteContext websiteContext)
        {
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
            this.pageLinkGenerator = pageLinkGenerator ?? throw new ArgumentNullException(nameof(pageLinkGenerator));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
            this.websiteContext = websiteContext ?? throw new ArgumentNullException(nameof(websiteContext));
        }

        #region FormController members

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

                if (!page.IsPublished)
                {
                    AddErrors("Нельзя создать коллекцию для страницы, которая не опубликована.");
                    return;
                }
            }
        }
        protected override async Task OnBuildFormAsync(PageCollectionCreateForm formModel)
        {
            if (page != null)
                formModel.Page = await GetPageModelAsync(page);

            formModel.Values.Sort = PageSortMode.FirstOld;
            formModel.Sorts = new List<ComboBoxItem>
            {
                new ComboBoxItem(PageSortMode.FirstOld.ToString(), "Сначало старые"),
                new ComboBoxItem(PageSortMode.FirstNew.ToString(), "Сначало новые")
            };
            formModel.PageTypes = pageMetadataManager.MetadataProviders.Select(it => new ComboBoxItem(it.Name, it.Title)).ToList();
        }
        protected override Task OnChangeValueAsync(string field, PageCollectionCreateValues values)
        {
            return Task.CompletedTask;
        }
        protected override async Task<PageCollectionModel> OnCommitAsync(PageCollectionCreateValues values)
        {
            Result<IPageCollection> createResult;

            if (page == null)
                createResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, values.Title, values.PageType, values.Sort);
            else
                createResult = await pageCollectionService.CreateCollectionAsync(page, values.Title, values.PageType, values.Sort);
            if (!createResult.IsSuccess)
            {
                AddErrors(createResult);
                return null;
            }

            return GetPageCollectionModel(createResult.Data);
        }

        #endregion

        #region Helper methods

        async Task<PageModel> GetPageModelAsync(IPage page)
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
        static PageCollectionModel GetPageCollectionModel(IPageCollection pageCollection)
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