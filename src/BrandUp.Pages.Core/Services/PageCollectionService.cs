using BrandUp.Pages.Metadata;
using BrandUp.Pages.Repositories;

namespace BrandUp.Pages.Services
{
    public class PageCollectionService : IPageCollectionService
    {
        readonly IPageCollectionRepository repositiry;
        readonly IPageRepository pageRepositiry;
        readonly IPageMetadataManager pageMetadataManager;

        public PageCollectionService(IPageCollectionRepository repositiry, IPageRepository pageRepositiry, IPageMetadataManager pageMetadataManager)
        {
            this.repositiry = repositiry ?? throw new ArgumentNullException(nameof(repositiry));
            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
        }

        public async Task<Result<IPageCollection>> CreateCollectionAsync(string webSiteId, string title, string pageTypeName, PageSortMode sortMode)
        {
            if (webSiteId == null)
                throw new ArgumentNullException(nameof(webSiteId));

            var collection = await repositiry.CreateCollectionAsync(webSiteId, title, pageTypeName, sortMode, null);

            return Result<IPageCollection>.Success(collection);
        }

        public async Task<Result<IPageCollection>> CreateCollectionAsync(IPage page, string title, string pageTypeName, PageSortMode sortMode)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (!page.IsPublished)
                return Result<IPageCollection>.Failed("Нельзя создать коллекцию страниц для страницы, которая не опубликована.");

            var collection = await repositiry.CreateCollectionAsync(page.WebsiteId, title, pageTypeName, sortMode, page.Id);

            return Result<IPageCollection>.Success(collection);
        }

        public Task<IPageCollection> FindCollectiondByIdAsync(Guid id)
        {
            return repositiry.FindCollectiondByIdAsync(id);
        }

        public Task<IEnumerable<IPageCollection>> ListCollectionsAsync(string webSiteId)
        {
            return repositiry.ListCollectionsAsync(webSiteId, null);
        }

        public Task<IEnumerable<IPageCollection>> ListCollectionsAsync(IPage page)
        {
            return repositiry.ListCollectionsAsync(page.WebsiteId, page.Id);
        }

        public Task<IEnumerable<IPageCollection>> FindCollectionsAsync(string webSiteId, string pageTypeName, string title = null, bool includeDerivedTypes = true)
        {
            if (pageTypeName == null)
                throw new ArgumentNullException(nameof(pageTypeName));

            var pageMetadata = pageMetadataManager.FindPageMetadataByName(pageTypeName);
            if (pageMetadata == null)
                throw new ArgumentException($"Тип страницы {pageTypeName} не существует.");

            var pageTypeNames = new List<string>
            {
                pageMetadata.Name
            };

            if (includeDerivedTypes)
            {
                foreach (var derivedPageMetadata in pageMetadata.GetDerivedMetadataWithHierarhy(false))
                    pageTypeNames.Add(derivedPageMetadata.Name);
            }

            return repositiry.FindCollectionsAsync(webSiteId, pageTypeNames.ToArray(), title);
        }

        public async Task<Result> UpdateCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default)
        {
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
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

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
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

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