using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageCollectionService
    {
        Task<Result<IPageCollection>> CreateCollectionAsync(string title, string pageTypeName, PageSortMode sortMode, Guid? pageId);
        Task<IPageCollection> FindCollectiondByIdAsync(Guid id);
        Task<IEnumerable<IPageCollection>> GetCollectionsAsync(Guid? pageId);
        Task<IEnumerable<IPageCollection>> GetCollectionsAsync(string pageTypeName, string title, bool includeDerivedTypes);
        Task<Result> UpdateCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default);
        Task<Result> DeleteCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default);
        Task<List<PageMetadataProvider>> GetPageTypesAsync(IPageCollection collection);
    }

    public interface IPageCollection
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        string Title { get; }
        string PageTypeName { get; }
        Guid? PageId { get; }
        PageSortMode SortMode { get; }

        void SetTitle(string newTitle);
        void SetSortModel(PageSortMode sortMode);
    }
}