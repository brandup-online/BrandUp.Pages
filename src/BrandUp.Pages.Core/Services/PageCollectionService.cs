using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageCollectionService : IPageCollectionService
    {
        private readonly IPageCollectionRepository repositiry;
        private readonly IPageRepository pageRepositiry;
        private readonly IPageMetadataManager pageMetadataManager;

        public PageCollectionService(IPageCollectionRepository repositiry, IPageRepository pageRepositiry, IPageMetadataManager pageMetadataManager)
        {
            this.repositiry = repositiry ?? throw new ArgumentNullException(nameof(repositiry));
            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
        }

        public async Task<Result<IPageCollection>> CreateCollectionAsync(string title, string pageTypeName, PageSortMode sortMode, Guid? pageId)
        {
            if (pageId.HasValue)
            {
                var page = await pageRepositiry.FindPageByIdAsync(pageId.Value);
                if (page == null)
                    throw new ArgumentException();

                if (!page.IsPublished)
                    return Result<IPageCollection>.Failed("Нельзя создать коллекцию страниц для страницы, которая не опубликована.");
            }

            var collection = await repositiry.CreateCollectionAsync(title, pageTypeName, sortMode, pageId);

            return Result<IPageCollection>.Success(collection);
        }

        public Task<IPageCollection> FindCollectiondByIdAsync(Guid id)
        {
            return repositiry.FindCollectiondByIdAsync(id);
        }

        public Task<IEnumerable<IPageCollection>> GetCollectionsAsync(Guid? pageId)
        {
            return repositiry.GetCollectionsAsync(pageId);
        }

        public Task<IEnumerable<IPageCollection>> GetCollectionsAsync(string pageTypeName, string title, bool includeDerivedTypes)
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

            return repositiry.GetCollectionsAsync(webSiteId, pageTypeNames.ToArray(), title);
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

            if (await pageRepositiry.HasPagesAsync(collection.Id))
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