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
        private readonly IPageMetadataManager pageTypeManager;
        private readonly Content.IContentMetadataManager contentMetadataManager;

        public PageService(
            IPageRepositiry pageRepositiry,
            IPageCollectionRepositiry pageCollectionRepositiry,
            IPageMetadataManager pageTypeManager,
            Content.IContentMetadataManager contentMetadataManager)
        {
            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
            this.pageCollectionRepositiry = pageCollectionRepositiry ?? throw new ArgumentNullException(nameof(pageCollectionRepositiry));
            this.pageTypeManager = pageTypeManager ?? throw new ArgumentNullException(nameof(pageTypeManager));
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
        }

        public async Task<IPage> CreatePageAsync(IPageCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var pageMetadata = pageTypeManager.FindPageMetadataByName(collection.PageTypeName);
            if (pageMetadata == null)
                throw new InvalidOperationException();

            var pageContentModel = pageMetadata.CreatePageModel();
            var pageContentData = contentMetadataManager.ConvertContentModelToDictionary(pageContentModel);

            return await pageRepositiry.CreatePageAsync(collection.Id, pageMetadata.Name, pageContentData);
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
            return pageRepositiry.GetDefaultPageAsync();
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

            var pageType = pageTypeManager.FindPageMetadataByName(page.TypeName);
            if (pageType == null)
                throw new InvalidOperationException();
            return Task.FromResult(pageType);
        }
        public async Task<object> GetPageContentAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var contentData = await pageRepositiry.GetContentAsync(page.Id);
            if (contentData == null)
                throw new InvalidOperationException();

            return contentMetadataManager.ConvertDictionaryToContentModel(contentData.Data);
        }
        public async Task PublishPageAsync(IPage page, string urlPathName)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (urlPathName == null)
                throw new ArgumentNullException(nameof(urlPathName));
            if (string.IsNullOrWhiteSpace(urlPathName))
                throw new ArgumentException();

            if (page.UrlPath != null)
                throw new InvalidOperationException("Страница уже опубликована.");

            var collection = await pageCollectionRepositiry.FindCollectiondByIdAsync(page.OwnCollectionId);

            string urlPath = urlPathName;
            if (collection.PageId.HasValue)
            {
                var parentPage = await pageRepositiry.FindPageByIdAsync(collection.PageId.Value);
                if (parentPage.UrlPath == null)
                    throw new InvalidOperationException("Нельзя опубликовать страницу, если родительская страница не опубликована.");
                urlPath = parentPage.UrlPath + "/" + urlPath;
            }

            if (await pageRepositiry.FindPageByPathAsync(urlPath) != null)
                throw new InvalidOperationException("Страница с таким url уже существует.");

            await pageRepositiry.SetUrlPathAsync(page.Id, urlPath);
        }
        public Task DeletePageAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            return pageRepositiry.DeletePageAsync(page.Id);
        }

        void IDisposable.Dispose()
        {
            if (pageRepositiry is IDisposable d)
                d.Dispose();
        }
    }
}