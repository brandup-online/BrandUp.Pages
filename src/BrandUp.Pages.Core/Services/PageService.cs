using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
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

            var basePageMetadata = pageMetadataManager.FindPageMetadataByName(collection.PageTypeName);
            if (basePageMetadata == null)
                throw new InvalidOperationException();

            var pageMetadata = pageMetadataManager.FindPageMetadataByName(pageType);
            if (pageMetadata == null)
                throw new ArgumentException();

            if (pageMetadata != basePageMetadata && !pageMetadata.IsInheritedOf(basePageMetadata))
                throw new ArgumentException();

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
            if (contentData == null)
                throw new InvalidOperationException();

            var pageMetadata = await GetPageTypeAsync(page);

            return pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(contentData.Data);
        }
        public async Task SetPageContentAsync(IPage page, object contentModel)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var pageMetadata = await GetPageTypeAsync(page);
            if (contentModel.GetType() != pageMetadata.ContentType)
                throw new ArgumentException();

            var pageData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(contentModel);

            await pageRepositiry.SetContentAsync(page.Id, new PageContent(1, pageData));
        }
        public async Task<Result> PublishPageAsync(IPage page, string urlPath)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (urlPath == null)
                throw new ArgumentNullException(nameof(urlPath));

            if (await IsPublishedAsync(page))
                return Result.Failed("Страница уже опубликована.");

            var urlPathValidationResult = pageUrlHelper.ValidateUrlPath(urlPath);
            if (!urlPathValidationResult.Succeeded)
                return urlPathValidationResult;

            var collection = await pageCollectionRepositiry.FindCollectiondByIdAsync(page.OwnCollectionId);
            if (collection.PageId.HasValue)
            {
                var parentPage = await pageRepositiry.FindPageByIdAsync(collection.PageId.Value);
                if (await IsPublishedAsync(parentPage))
                    return Result.Failed("Нельзя опубликовать страницу, если родительская страница не опубликована.");
                urlPath = pageUrlHelper.ExtendUrlPath(parentPage.UrlPath, urlPath);
            }
            else
                urlPath = pageUrlHelper.NormalizeUrlPath(urlPath);

            if (await pageRepositiry.FindPageByPathAsync(urlPath) != null)
                return Result.Failed("Страница с таким url уже существует.");

            await pageRepositiry.SetUrlPathAsync(page.Id, urlPath);

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
        public Task<bool> IsPublishedAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            return Task.FromResult(page.UrlPath != null);
        }
    }
}