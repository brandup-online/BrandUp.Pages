using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageService : IPageService
    {
        private readonly IPageRepositiry pageRepositiry;
        private readonly IPageCollectionRepositiry pageCollectionRepositiry;
        private readonly IPageMetadataManager pageMetadataManager;
        private readonly Url.IPageUrlHelper pageUrlHelper;

        public PageService(
            IPageRepositiry pageRepositiry,
            IPageCollectionRepositiry pageCollectionRepositiry,
            IPageMetadataManager pageMetadataManager,
            Url.IPageUrlHelper pageUrlHelper)
        {
            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
            this.pageCollectionRepositiry = pageCollectionRepositiry ?? throw new ArgumentNullException(nameof(pageCollectionRepositiry));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
            this.pageUrlHelper = pageUrlHelper ?? throw new ArgumentNullException(nameof(pageUrlHelper));
        }

        public async Task<IPage> CreatePageAsync(IPageCollection collection, string pageType = null, string pageTitle = null)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (pageType == null)
                pageType = collection.PageTypeName;

            var basePageMetadata = pageMetadataManager.GetMetadata(collection.PageTypeName);
            var pageMetadata = pageMetadataManager.GetMetadata(pageType);

            if (!pageMetadata.IsInheritedOrEqual(basePageMetadata))
                throw new ArgumentException($"Тип страницы {pageType} не подходит для коллекции {collection.Title} ({collection.Id}).");

            var pageContentModel = pageMetadata.CreatePageModel(pageTitle);
            var pageContentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(pageContentModel);

            pageTitle = pageMetadata.GetPageTitle(pageContentModel);

            return await pageRepositiry.CreatePageAsync(collection.Id, pageMetadata.Name, pageTitle, pageContentData);
        }
        public Task<IPage> FindPageByIdAsync(Guid id)
        {
            return pageRepositiry.FindPageByIdAsync(id);
        }
        public Task<IPage> FindPageByPathAsync(string pagePath)
        {
            if (pagePath == null)
                throw new ArgumentNullException(nameof(pagePath));

            if (pagePath.Length > 0)
                pagePath = pagePath.Trim(new char[] { '/' });

            if (pagePath == string.Empty)
                return GetDefaultPageAsync();

            return pageRepositiry.FindPageByPathAsync(pagePath);
        }
        public Task<IPage> GetDefaultPageAsync()
        {
            return pageRepositiry.FindPageByPathAsync(pageUrlHelper.GetDefaultPagePath());
        }
        public Task<IEnumerable<IPage>> GetPagesAsync(IPageCollection collection, PagePaginationOptions pagination)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));

            return pageRepositiry.GetPagesAsync(collection.Id, collection.SortMode, pagination);
        }
        public Task<IEnumerable<IPage>> SearchPagesAsync(string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            if (title.Length < 3)
                throw new ArgumentOutOfRangeException(nameof(title));
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));

            return pageRepositiry.SearchPagesAsync(title, pagination, cancellationToken);
        }
        public Task<PageMetadataProvider> GetPageTypeAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var pageType = pageMetadataManager.FindPageMetadataByName(page.TypeName);
            if (pageType == null)
                throw new InvalidOperationException($"Тип страницы {page.TypeName} не зарегистрирован.");
            return Task.FromResult(pageType);
        }
        public async Task<object> GetPageContentAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var contentData = await pageRepositiry.GetContentAsync(page.Id);
            var pageMetadata = await GetPageTypeAsync(page);

            if (contentData != null)
                return pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(contentData.Data);
            else
                return pageMetadata.CreatePageModel();
        }
        public async Task SetPageContentAsync(IPage page, object contentModel)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var pageMetadata = await GetPageTypeAsync(page);
            if (contentModel.GetType() != pageMetadata.ContentType)
                throw new ArgumentException();

            var pageTitle = pageMetadata.GetPageTitle(contentModel);
            var pageData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(contentModel);

            await pageRepositiry.SetContentAsync(page.Id, pageTitle, new PageContent(1, pageData));

            page.Title = pageTitle;
        }
        public async Task<Result> PublishPageAsync(IPage page, string urlPath)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (urlPath == null)
                throw new ArgumentNullException(nameof(urlPath));

            if (page.IsPublished)
                return Result.Failed("Страница уже опубликована.");

            var urlPathValidationResult = pageUrlHelper.ValidateUrlPath(urlPath);
            if (!urlPathValidationResult.Succeeded)
                return urlPathValidationResult;

            var collection = await pageCollectionRepositiry.FindCollectiondByIdAsync(page.OwnCollectionId);
            if (collection.PageId.HasValue)
            {
                var parentPage = await pageRepositiry.FindPageByIdAsync(collection.PageId.Value);
                if (!parentPage.IsPublished)
                    return Result.Failed("Нельзя опубликовать страницу, если родительская страница не опубликована.");
                urlPath = pageUrlHelper.ExtendUrlPath(parentPage.UrlPath, urlPath);
            }
            else
                urlPath = pageUrlHelper.NormalizeUrlPath(urlPath);

            if (await pageRepositiry.FindPageByPathAsync(urlPath) != null)
                return Result.Failed("Страница с таким url уже существует.");

            await page.SetUrlAsync(urlPath);

            await pageRepositiry.UpdatePageAsync(page);

            return Result.Success;
        }
        public async Task<Result> DeletePageAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            try
            {
                await pageRepositiry.DeletePageAsync(page.Id);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failed(ex);
            }
        }
        public async Task<Guid?> GetParentPageIdAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var pageCollection = await pageCollectionRepositiry.FindCollectiondByIdAsync(page.OwnCollectionId);
            return pageCollection.PageId;
        }
    }
}