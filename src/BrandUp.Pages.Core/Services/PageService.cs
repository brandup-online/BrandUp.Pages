using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageService : IPageService
    {
        readonly IPageRepository pageRepositiry;
        readonly IPageCollectionRepository pageCollectionRepositiry;
        readonly IPageMetadataManager pageMetadataManager;
        readonly Url.IPageUrlHelper pageUrlHelper;
        readonly Views.IViewLocator viewLocator;
        readonly PagesOptions options;

        public PageService(
            IPageRepository pageRepositiry,
            IPageCollectionRepository pageCollectionRepositiry,
            IPageMetadataManager pageMetadataManager,
            Url.IPageUrlHelper pageUrlHelper,
            Views.IViewLocator viewLocator,
            IOptions<PagesOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
            this.pageCollectionRepositiry = pageCollectionRepositiry ?? throw new ArgumentNullException(nameof(pageCollectionRepositiry));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
            this.pageUrlHelper = pageUrlHelper ?? throw new ArgumentNullException(nameof(pageUrlHelper));
            this.viewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));
            this.options = options.Value;
        }

        public async Task<IPage> CreatePageAsync(IPageCollection collection, object pageContent, CancellationToken cancellationToken = default)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var basePageMetadata = pageMetadataManager.GetMetadata(collection.PageTypeName);
            var pageMetadata = pageMetadataManager.GetMetadata(pageContent.GetType());

            if (!pageMetadata.AllowCreateModel)
                throw new InvalidOperationException($"Нельзя создать страницу с типом {pageMetadata.Name}, так как её тип контент является абстрактным.");
            if (!pageMetadata.IsInheritedOrEqual(basePageMetadata))
                throw new ArgumentException($"Тип страницы {pageMetadata.Name} не подходит для коллекции {collection.Title} ({collection.Id}).");

            var pageContentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(pageContent);
            var pageHeader = pageMetadata.GetPageHeader(pageContent);

            var page = await pageRepositiry.CreatePageAsync(collection.Id, pageMetadata.Name, pageHeader, pageContentData, cancellationToken);

            if (collection.CustomSorting)
            {
                switch (collection.SortMode)
                {
                    case PageSortMode.FirstNew:
                        {
                            await UpPagePositionAsync(page, null, cancellationToken);
                            break;
                        }
                    case PageSortMode.FirstOld:
                        {
                            await DownPagePositionAsync(page, null, cancellationToken);
                            break;
                        }
                    default:
                        throw new Exception();
                }
            }

            return page;
        }
        public async Task<IPage> CreatePageAsync(IPageCollection collection, string pageType = null, string pageHeader = null, CancellationToken cancellationToken = default)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (pageType == null)
                pageType = collection.PageTypeName;

            var basePageMetadata = pageMetadataManager.GetMetadata(collection.PageTypeName);
            var pageMetadata = pageMetadataManager.GetMetadata(pageType);

            if (!pageMetadata.AllowCreateModel)
                throw new InvalidOperationException($"Нельзя создать страницу с типом {pageMetadata.Name}, так как её тип контент является абстрактным.");
            if (!pageMetadata.IsInheritedOrEqual(basePageMetadata))
                throw new ArgumentException($"Тип страницы {pageType} не подходит для коллекции {collection.Title} ({collection.Id}).");

            var pageContent = pageMetadata.CreatePageModel();

            ApplyDefaultDataToContentModel(pageMetadata, pageContent, pageHeader);

            var pageContentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(pageContent);
            pageHeader = pageMetadata.GetPageHeader(pageContent);

            var page = await pageRepositiry.CreatePageAsync(collection.Id, pageMetadata.Name, pageHeader, pageContentData, cancellationToken);

            if (collection.CustomSorting)
            {
                switch (collection.SortMode)
                {
                    case PageSortMode.FirstNew:
                        {
                            await UpPagePositionAsync(page, null, cancellationToken);
                            break;
                        }
                    case PageSortMode.FirstOld:
                        {
                            await DownPagePositionAsync(page, null, cancellationToken);
                            break;
                        }
                    default:
                        throw new Exception();
                }
            }

            return page;
        }
        public Task<IPage> FindPageByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return pageRepositiry.FindPageByIdAsync(id, cancellationToken);
        }
        public Task<IPage> FindPageByPathAsync(string pagePath, CancellationToken cancellationToken = default)
        {
            if (pagePath == null)
                throw new ArgumentNullException(nameof(pagePath));

            pagePath = pageUrlHelper.NormalizeUrlPath(pagePath);
            if (pagePath == string.Empty)
                return GetDefaultPageAsync();

            return pageRepositiry.FindPageByPathAsync(pagePath, cancellationToken);
        }
        public Task<PageUrlResult> FindPageUrlAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            path = pageUrlHelper.NormalizeUrlPath(path);
            if (path == string.Empty)
                path = pageUrlHelper.GetDefaultPagePath();

            return pageRepositiry.FindPageUrlAsync(path, cancellationToken);
        }
        public Task<IPage> GetDefaultPageAsync(CancellationToken cancellationToken = default)
        {
            return pageRepositiry.FindPageByPathAsync(pageUrlHelper.GetDefaultPagePath(), cancellationToken);
        }
        public async Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var collection = await pageCollectionRepositiry.FindCollectiondByIdAsync(options.CollectionId);
            if (collection == null)
                throw new InvalidOperationException();

            if (!options.SortDirection.HasValue)
                options.SortDirection = collection.SortMode;

            if (!options.CustomSorting.HasValue)
                options.CustomSorting = collection.CustomSorting;

            return await pageRepositiry.GetPagesAsync(options, cancellationToken);
        }
        public Task<IEnumerable<IPage>> GetPublishedPagesAsync(CancellationToken cancellationToken = default)
        {
            return pageRepositiry.GetPublishedPagesAsync(cancellationToken);
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
        public Task<PageMetadataProvider> GetPageTypeAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var pageType = pageMetadataManager.FindPageMetadataByName(page.TypeName);
            if (pageType == null)
                throw new InvalidOperationException($"Тип страницы {page.TypeName} не зарегистрирован.");
            return Task.FromResult(pageType);
        }
        public async Task<object> GetPageContentAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var contentData = await pageRepositiry.GetContentAsync(page.Id, cancellationToken);
            if (contentData == null)
                throw new InvalidOperationException("Для страницы не задан контент.");
            var pageMetadata = await GetPageTypeAsync(page, cancellationToken);

            return pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(contentData);
        }
        public async Task SetPageContentAsync(IPage page, object contentModel, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var pageMetadata = await GetPageTypeAsync(page, cancellationToken);
            if (contentModel.GetType() != pageMetadata.ContentType)
                throw new ArgumentException();

            var pageTitle = pageMetadata.GetPageHeader(contentModel);
            var pageData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(contentModel);

            await pageRepositiry.SetContentAsync(page.Id, pageTitle, pageData, cancellationToken);

            page.Header = pageTitle;
        }
        public async Task<Result> PublishPageAsync(IPage page, string urlPath, CancellationToken cancellationToken = default)
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
                var parentPage = await pageRepositiry.FindPageByIdAsync(collection.PageId.Value, cancellationToken);
                if (!parentPage.IsPublished)
                    return Result.Failed("Нельзя опубликовать страницу, если родительская страница не опубликована.");
                urlPath = pageUrlHelper.ExtendUrlPath(parentPage.UrlPath, urlPath);
            }
            else
                urlPath = pageUrlHelper.NormalizeUrlPath(urlPath);

            if (await pageRepositiry.FindPageByPathAsync(urlPath) != null)
                return Result.Failed("Страница с таким url уже существует.");

            await page.SetUrlAsync(urlPath);
            await pageRepositiry.UpdatePageAsync(page, cancellationToken);

            return Result.Success;
        }
        public async Task<Result> DeletePageAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            try
            {
                await pageRepositiry.DeletePageAsync(page, cancellationToken);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failed(ex);
            }
        }
        public async Task<Guid?> GetParentPageIdAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var pageCollection = await pageCollectionRepositiry.FindCollectiondByIdAsync(page.OwnCollectionId);
            return pageCollection.PageId;
        }
        public async Task<PageSeoOptions> GetPageSeoOptionsAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var result = new PageSeoOptions
            {
                Title = await pageRepositiry.GetPageTitleAsync(page, cancellationToken),
                Description = await pageRepositiry.GetPageDescriptionAsync(page, cancellationToken),
                Keywords = await pageRepositiry.GetPageKeywordsAsync(page, cancellationToken)
            };

            return result;
        }
        public async Task UpdatePageSeoOptionsAsync(IPage page, PageSeoOptions seoOptions, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (seoOptions == null)
                throw new ArgumentNullException(nameof(seoOptions));

            await pageRepositiry.SetPageTitleAsync(page, seoOptions.Title);
            await pageRepositiry.SetPageDescriptionAsync(page, seoOptions.Description);
            await pageRepositiry.SetPageKeywordsAsync(page, seoOptions.Keywords);

            await pageRepositiry.UpdatePageAsync(page, cancellationToken);
        }
        public Task UpPagePositionAsync(IPage page, IPage beforePage, CancellationToken cancellationToken = default)
        {
            return pageRepositiry.UpPagePositionAsync(page, beforePage, cancellationToken);
        }
        public Task DownPagePositionAsync(IPage page, IPage afterPage, CancellationToken cancellationToken = default)
        {
            return pageRepositiry.DownPagePositionAsync(page, afterPage, cancellationToken);
        }

        private void ApplyDefaultDataToContentModel(PageMetadataProvider pageMetadataProvider, object contentModel, string header = null)
        {
            if (string.IsNullOrEmpty(header) && !string.IsNullOrEmpty(options.DefaultPageHeader))
                pageMetadataProvider.SetPageHeader(contentModel, options.DefaultPageHeader);

            var view = viewLocator.FindView(pageMetadataProvider.ContentType);
            if (view == null)
                throw new InvalidOperationException();

            if (view.DefaultModelData != null)
                pageMetadataProvider.ContentMetadata.ApplyDataToModel(view.DefaultModelData, contentModel);

            if (string.IsNullOrEmpty(header))
                header = pageMetadataProvider.Title;

            pageMetadataProvider.SetPageHeader(contentModel, header);
        }
    }
}