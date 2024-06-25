﻿using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.Extensions.Options;

namespace BrandUp.Pages.Services
{
    public class PageService(
        IPageRepository pageRepository,
        IPageCollectionRepository pageCollectionRepository,
        IPageMetadataManager pageMetadataManager,
        Url.IPageUrlHelper pageUrlHelper,
        Views.IViewLocator viewLocator,
        ContentService contentService,
        IOptions<PagesOptions> options) : IPageService
    {
        readonly PagesOptions options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        public async Task<string> GetContentKeyAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult($"page-{pageId}");
        }
        public async Task<IPage> CreatePageAsync(IPageCollection collection, object pageContent, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(collection);
            ArgumentNullException.ThrowIfNull(pageContent);

            var basePageMetadata = pageMetadataManager.GetMetadata(collection.PageTypeName);
            var pageMetadata = pageMetadataManager.GetMetadata(pageContent.GetType());

            if (!pageMetadata.AllowCreateModel)
                throw new InvalidOperationException($"Нельзя создать страницу с типом {pageMetadata.Name}, так как её тип контент является абстрактным.");
            if (!pageMetadata.IsInheritedOrEqual(basePageMetadata))
                throw new ArgumentException($"Тип страницы {pageMetadata.Name} не подходит для коллекции {collection.Title} ({collection.Id}).");

            var pageHeader = pageMetadata.GetPageHeader(pageContent);

            var pageId = Guid.NewGuid();
            var pageContentKey = await GetContentKeyAsync(pageId, cancellationToken);
            var page = await pageRepository.CreatePageAsync(collection.WebsiteId, collection.Id, pageId, pageMetadata.Name, pageHeader, cancellationToken);

            await contentService.SetContentAsync(collection.WebsiteId, pageContentKey, pageContent, cancellationToken);

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
            ArgumentNullException.ThrowIfNull(collection);

            pageType ??= collection.PageTypeName;

            var basePageMetadata = pageMetadataManager.GetMetadata(collection.PageTypeName);
            var pageMetadata = pageMetadataManager.GetMetadata(pageType);

            if (!pageMetadata.AllowCreateModel)
                throw new InvalidOperationException($"Нельзя создать страницу с типом {pageMetadata.Name}, так как её тип контент является абстрактным.");
            if (!pageMetadata.IsInheritedOrEqual(basePageMetadata))
                throw new ArgumentException($"Тип страницы {pageType} не подходит для коллекции {collection.Title} ({collection.Id}).");

            //var pageContent = pageMetadata.CreatePageModel();

            //ApplyDefaultDataToContentModel(pageMetadata, pageContent, pageHeader);

            //var pageContentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(pageContent);
            //pageHeader = pageMetadata.GetPageHeader(pageContent);

            var pageId = Guid.NewGuid();
            var page = await pageRepository.CreatePageAsync(collection.WebsiteId, collection.Id, pageId, pageMetadata.Name, pageHeader, cancellationToken);

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
            return pageRepository.FindPageByIdAsync(id, cancellationToken);
        }
        public Task<IPage> FindPageByPathAsync(string webSiteId, string path, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(webSiteId);
            ArgumentNullException.ThrowIfNull(path);

            path = pageUrlHelper.NormalizeUrlPath(path);
            if (path == string.Empty)
                return GetDefaultPageAsync(webSiteId, cancellationToken);

            return pageRepository.FindPageByPathAsync(webSiteId, path, cancellationToken);
        }
        public Task<PageUrlResult> FindUrlByPathAsync(string webSiteId, string path, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(webSiteId);
            ArgumentNullException.ThrowIfNull(path);

            path = pageUrlHelper.NormalizeUrlPath(path);
            if (path == string.Empty)
                path = pageUrlHelper.GetDefaultPagePath();

            return pageRepository.FindUrlByPathAsync(webSiteId, path, cancellationToken);
        }
        public Task<IPage> GetDefaultPageAsync(string webSiteId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(webSiteId);

            return pageRepository.FindPageByPathAsync(webSiteId, pageUrlHelper.GetDefaultPagePath(), cancellationToken);
        }
        public async Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            var collection = await pageCollectionRepository.FindCollectiondByIdAsync(options.CollectionId);
            if (collection == null)
                throw new InvalidOperationException();

            if (!options.SortDirection.HasValue)
                options.SortDirection = collection.SortMode;

            if (!options.CustomSorting.HasValue)
                options.CustomSorting = collection.CustomSorting;

            return await pageRepository.GetPagesAsync(options, cancellationToken);
        }
        public Task<IEnumerable<IPage>> GetPublishedPagesAsync(string webSiteId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(webSiteId);

            return pageRepository.GetPublishedPagesAsync(webSiteId, cancellationToken);
        }
        public Task<IEnumerable<IPage>> SearchPagesAsync(string webSiteId, string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(webSiteId);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(pagination);

            if (title.Length < 3)
                throw new ArgumentOutOfRangeException(nameof(title));

            return pageRepository.SearchPagesAsync(webSiteId, title, pagination, cancellationToken);
        }
        public Task<PageMetadataProvider> GetPageTypeAsync(IPage page, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);

            var pageType = pageMetadataManager.FindPageMetadataByName(page.TypeName);
            if (pageType == null)
                throw new InvalidOperationException($"Тип страницы {page.TypeName} не зарегистрирован.");
            return Task.FromResult(pageType);
        }
        public async Task<Result> PublishPageAsync(IPage page, string urlPath, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(urlPath);

            if (page.IsPublished)
                return Result.Failed("Страница уже опубликована.");

            var urlPathValidationResult = pageUrlHelper.ValidateUrlPath(urlPath);
            if (!urlPathValidationResult.IsSuccess)
                return urlPathValidationResult;

            var collection = await pageCollectionRepository.FindCollectiondByIdAsync(page.OwnCollectionId);
            if (collection.PageId.HasValue)
            {
                var parentPage = await pageRepository.FindPageByIdAsync(collection.PageId.Value, cancellationToken);
                if (!parentPage.IsPublished)
                    return Result.Failed("Нельзя опубликовать страницу, если родительская страница не опубликована.");
                urlPath = pageUrlHelper.ExtendUrlPath(parentPage.UrlPath, urlPath);
            }
            else
                urlPath = pageUrlHelper.NormalizeUrlPath(urlPath);

            if (await pageRepository.FindPageByPathAsync(page.WebsiteId, urlPath, cancellationToken) != null)
                return Result.Failed("Страница с таким url уже существует.");

            await pageRepository.SetUrlPathAsync(page, urlPath, cancellationToken);
            await pageRepository.UpdatePageAsync(page, cancellationToken);

            return Result.Success;
        }
        public async Task<Result> DeletePageAsync(IPage page, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);

            var pageContentKey = await GetContentKeyAsync(page.Id, cancellationToken);

            await pageRepository.DeletePageAsync(page, pageContentKey, cancellationToken);

            return Result.Success;
        }
        public async Task<Guid?> GetParentPageIdAsync(IPage page, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);

            var pageCollection = await pageCollectionRepository.FindCollectiondByIdAsync(page.OwnCollectionId);
            return pageCollection.PageId;
        }
        public async Task<PageSeoOptions> GetPageSeoOptionsAsync(IPage page, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);

            var result = new PageSeoOptions
            {
                Title = await pageRepository.GetPageTitleAsync(page, cancellationToken),
                Description = await pageRepository.GetPageDescriptionAsync(page, cancellationToken),
                Keywords = await pageRepository.GetPageKeywordsAsync(page, cancellationToken)
            };

            return result;
        }
        public async Task UpdatePageSeoOptionsAsync(IPage page, PageSeoOptions seoOptions, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(seoOptions);

            await pageRepository.SetPageTitleAsync(page, seoOptions.Title, cancellationToken);
            await pageRepository.SetPageDescriptionAsync(page, seoOptions.Description, cancellationToken);
            await pageRepository.SetPageKeywordsAsync(page, seoOptions.Keywords, cancellationToken);

            await pageRepository.UpdatePageAsync(page, cancellationToken);
        }
        public Task UpPagePositionAsync(IPage page, IPage beforePage, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(beforePage);

            return pageRepository.UpPagePositionAsync(page, beforePage, cancellationToken);
        }
        public Task DownPagePositionAsync(IPage page, IPage afterPage, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(afterPage);

            return pageRepository.DownPagePositionAsync(page, afterPage, cancellationToken);
        }

        void ApplyDefaultDataToContentModel(PageMetadataProvider pageMetadataProvider, object contentModel, string header = null)
        {
            ArgumentNullException.ThrowIfNull(pageMetadataProvider);
            ArgumentNullException.ThrowIfNull(contentModel);

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