using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;

namespace BrandUp.Pages.Services
{
    public class PageCollectionService(IPageCollectionRepository repositiry, IPageRepository pageRepositiry, IPageMetadataManager pageMetadataManager) : IPageCollectionService
    {
        public async Task<Result<IPageCollection>> CreateCollectionAsync(string websiteId, string title, string pageTypeName, PageSortMode sortMode)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(pageTypeName);

            var collection = await repositiry.CreateCollectionAsync(websiteId, title, pageTypeName, sortMode, null);

            return Result<IPageCollection>.Success(collection);
        }

        public async Task<Result<IPageCollection>> CreateCollectionAsync(IPage page, string title, string pageTypeName, PageSortMode sortMode)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(pageTypeName);

            if (!page.IsPublished)
                return Result<IPageCollection>.Failed("Нельзя создать коллекцию страниц для страницы, которая не опубликована.");

            var collection = await repositiry.CreateCollectionAsync(page.WebsiteId, title, pageTypeName, sortMode, page.Id);

            return Result<IPageCollection>.Success(collection);
        }

        public Task<IPageCollection> FindCollectiondByIdAsync(Guid id)
        {
            return repositiry.FindCollectiondByIdAsync(id);
        }

        public Task<IEnumerable<IPageCollection>> ListCollectionsAsync(string websiteId)
        {
            ArgumentNullException.ThrowIfNull(websiteId);

            return repositiry.ListCollectionsAsync(websiteId, null);
        }

        public Task<IEnumerable<IPageCollection>> ListCollectionsAsync(IPage page)
        {
            ArgumentNullException.ThrowIfNull(page);

            return repositiry.ListCollectionsAsync(page.WebsiteId, page.Id);
        }

        public Task<IEnumerable<IPageCollection>> FindCollectionsAsync(string websiteId, string pageTypeName, string title = null, bool includeDerivedTypes = true)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(pageTypeName);

            var pageMetadata = pageMetadataManager.FindPageMetadataByName(pageTypeName);
            if (pageMetadata == null)
                throw new ArgumentException($"Тип страницы {pageTypeName} не существует.");

            var pageTypeNames = new List<string> { pageMetadata.Name };

            if (includeDerivedTypes)
            {
                foreach (var derivedPageMetadata in pageMetadata.GetDerivedMetadataWithHierarhy(false))
                    pageTypeNames.Add(derivedPageMetadata.Name);
            }

            return repositiry.FindCollectionsAsync(websiteId, [.. pageTypeNames], title);
        }

        public async Task<Result> UpdateCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(collection);

            try
            {
                await repositiry.UpdateCollectionAsync(collection, cancellationToken);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failed(ex);
            }
        }

        public async Task<Result> DeleteCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(collection);

            if (await pageRepositiry.HasPagesAsync(collection.Id, cancellationToken))
                return Result.Failed("Нельзя удалить коллекцию страниц, которая содержит страницы.");

            try
            {
                await repositiry.DeleteCollectionAsync(collection, cancellationToken);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failed(ex);
            }
        }

        public Task<List<PageMetadataProvider>> GetPageTypesAsync(IPageCollection collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            var basePageType = pageMetadataManager.FindPageMetadataByName(collection.PageTypeName);
            if (basePageType == null)
                throw new InvalidOperationException();

            var result = new List<PageMetadataProvider>();
            foreach (var pageType in basePageType.GetDerivedMetadataWithHierarhy(true))
            {
                if (pageType.ContentType.IsAbstract)
                    continue;

                result.Add(pageType);
            }
            return Task.FromResult(result);
        }
    }
}