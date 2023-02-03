using BrandUp.Pages.Items;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        readonly IServiceProvider serviceProvider;
        readonly IOptionsSnapshot<ItemPageOptions> itemOptions;

        public PageService(
            IPageRepository pageRepositiry,
            IPageCollectionRepository pageCollectionRepositiry,
            IPageMetadataManager pageMetadataManager,
            Url.IPageUrlHelper pageUrlHelper,
            Views.IViewLocator viewLocator,
            IOptions<PagesOptions> options,
            IServiceProvider serviceProvider,
            IOptionsSnapshot<ItemPageOptions> itemOptions)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
            this.pageCollectionRepositiry = pageCollectionRepositiry ?? throw new ArgumentNullException(nameof(pageCollectionRepositiry));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
            this.pageUrlHelper = pageUrlHelper ?? throw new ArgumentNullException(nameof(pageUrlHelper));
            this.viewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));
            this.options = options.Value;
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.itemOptions = itemOptions ?? throw new ArgumentNullException(nameof(itemOptions));
        }

        public async Task<IPage> CreatePageByItemAsync<TItem, TContent>(string webSiteId, TItem item, TContent pageContent, CancellationToken cancellationToken = default)
            where TItem : class
            where TContent : class, new()
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (pageContent == null)
                throw new ArgumentNullException(nameof(pageContent));

            var pageMetadata = pageMetadataManager.GetMetadata(pageContent.GetType());
            if (!pageMetadata.AllowCreateModel)
                throw new InvalidOperationException($"Нельзя создать страницу с типом {pageMetadata.Name}, так как её тип контент является абстрактным.");

            var itemProvider = serviceProvider.GetRequiredService<IItemProvider<TItem>>();
            var itemId = await itemProvider.GetIdAsync(item, cancellationToken);
            if (string.IsNullOrEmpty(itemId))
                throw new InvalidOperationException("ID элемента страницы не может быть пустым.");
            var itemTypeName = typeof(TItem).FullName;

            var pageContentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(pageContent);
            var pageHeader = pageMetadata.GetPageHeader(pageContent);

            return await pageRepositiry.CreatePageByItemAsync(webSiteId, itemId, itemTypeName, pageMetadata.Name, pageHeader, pageContentData, cancellationToken);
        }
        public async Task<IPage> FindPageByItemAsync<TItem>(string webSiteId, TItem item, CancellationToken cancellationToken = default)
            where TItem : class
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var itemProvider = serviceProvider.GetRequiredService<IItemProvider<TItem>>();
            var itemId = await itemProvider.GetIdAsync(item, cancellationToken);

            return await pageRepositiry.FindPageByItemAsync(webSiteId, itemId, cancellationToken);
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

            var page = await pageRepositiry.CreatePageAsync(collection.WebsiteId, collection.Id, pageMetadata.Name, pageHeader, pageContentData, cancellationToken);

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

            pageType ??= collection.PageTypeName;

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

            var page = await pageRepositiry.CreatePageAsync(collection.WebsiteId, collection.Id, pageMetadata.Name, pageHeader, pageContentData, cancellationToken);

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
        public Task<IPage> FindPageByPathAsync(string webSiteId, string pagePath, CancellationToken cancellationToken = default)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));
            if (pagePath == null)
                throw new ArgumentNullException(nameof(pagePath));

            pagePath = pageUrlHelper.NormalizeUrlPath(pagePath);
            if (pagePath == string.Empty)
                return GetDefaultPageAsync(webSiteId, cancellationToken);

            return pageRepositiry.FindPageByPathAsync(webSiteId, pagePath, cancellationToken);
        }
        public Task<PageUrlResult> FindUrlByPathAsync(string webSiteId, string path, CancellationToken cancellationToken = default)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            path = pageUrlHelper.NormalizeUrlPath(path);
            if (path == string.Empty)
                path = pageUrlHelper.GetDefaultPagePath();

            return pageRepositiry.FindUrlByPathAsync(webSiteId, path, cancellationToken);
        }
        public Task<IPage> GetDefaultPageAsync(string webSiteId, CancellationToken cancellationToken = default)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));

            return pageRepositiry.FindPageByPathAsync(webSiteId, pageUrlHelper.GetDefaultPagePath(), cancellationToken);
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
        public Task<IEnumerable<IPage>> GetPublishedPagesAsync(string webSiteId, CancellationToken cancellationToken = default)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));

            return pageRepositiry.GetPublishedPagesAsync(webSiteId, cancellationToken);
        }
        public Task<IEnumerable<IPage>> SearchPagesAsync(string webSiteId, string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            if (title.Length < 3)
                throw new ArgumentOutOfRangeException(nameof(title));
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));

            return pageRepositiry.SearchPagesAsync(webSiteId, title, pagination, cancellationToken);
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
            var contentModelType = contentModel.GetType();
            if (contentModelType != pageMetadata.ContentType)
                throw new ArgumentException($"Тип контента страницы должен наследовать {pageMetadata.ContentType.AssemblyQualifiedName}.", nameof(contentModel));

            var pageHeader = pageMetadata.GetPageHeader(contentModel);
            var pageData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(contentModel);

            await pageRepositiry.SetContentAsync(page.Id, pageHeader, pageData, cancellationToken);

            if (page.ItemId != null)
            {
                var itemPageOptions = itemOptions.Get(page.ItemType);
                if (itemPageOptions == null)
                    throw new InvalidOperationException($"Не найдена конфигурация элементов с типом {page.ItemType}.");
                var itemProvider = serviceProvider.GetRequiredService(itemPageOptions.ItemProviderType);
                if (itemProvider is IPageCallbacks pageCallbacks)
                    await pageCallbacks.UpdateHeaderAsync(page.ItemId, page.ItemType, cancellationToken);
            }

            page.Header = pageHeader;
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
            if (!urlPathValidationResult.IsSuccess)
                return urlPathValidationResult;

            string normalizedUrlPath;
            if (page.ItemId == null)
            {
                var collection = await pageCollectionRepositiry.FindCollectiondByIdAsync(page.OwnCollectionId);
                if (collection.PageId.HasValue)
                {
                    var parentPage = await pageRepositiry.FindPageByIdAsync(collection.PageId.Value, cancellationToken);
                    if (!parentPage.IsPublished)
                        return Result.Failed("Нельзя опубликовать страницу, если родительская страница не опубликована.");

                    normalizedUrlPath = pageUrlHelper.ExtendUrlPath(parentPage.UrlPath, urlPath);
                }
                else
                    normalizedUrlPath = pageUrlHelper.NormalizeUrlPath(urlPath);
            }
            else
            {
                normalizedUrlPath = string.Concat(page.ItemType.ToLower(), ":", pageUrlHelper.NormalizeUrlPath(urlPath));
            }

            if (await pageRepositiry.FindPageByPathAsync(page.WebsiteId, normalizedUrlPath, cancellationToken) != null)
                return Result.Failed("Страница с таким url уже существует.");

            await pageRepositiry.SetUrlPathAsync(page, normalizedUrlPath, cancellationToken);
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
            if (page.ItemId != null)
                return null;

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

            await pageRepositiry.SetPageTitleAsync(page, seoOptions.Title, cancellationToken);
            await pageRepositiry.SetPageDescriptionAsync(page, seoOptions.Description, cancellationToken);
            await pageRepositiry.SetPageKeywordsAsync(page, seoOptions.Keywords, cancellationToken);

            await pageRepositiry.UpdatePageAsync(page, cancellationToken);
        }
        public Task UpPagePositionAsync(IPage page, IPage beforePage, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (beforePage == null)
                throw new ArgumentNullException(nameof(beforePage));
            if (page.ItemId != null)
                throw new InvalidOperationException("Страницы связанные с элементами не поддерживают ручную сортировку.");

            return pageRepositiry.UpPagePositionAsync(page, beforePage, cancellationToken);
        }
        public Task DownPagePositionAsync(IPage page, IPage afterPage, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (afterPage == null)
                throw new ArgumentNullException(nameof(afterPage));
            if (page.ItemId != null)
                throw new InvalidOperationException("Страницы связанные с элементами не поддерживают ручную сортировку.");

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