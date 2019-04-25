using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageCollectionService
    {
        Task<IPageCollection> CreateCollectionAsync(string title, string pageTypeName, PageSortMode sortMode, Guid? pageId);
        Task<IPageCollection> FindCollectiondByIdAsync(Guid id);
        Task<IEnumerable<IPageCollection>> GetCollectionsAsync(Guid? pageId);
        Task<IEnumerable<IPageCollection>> GetCollectionsAsync(string pageTypeName, bool includeDerivedTypes);
        Task<IPageCollection> UpdateCollectionAsync(Guid id, string title, PageSortMode pageSort);
        Task<Result> DeleteCollectionAsync(IPageCollection collection);
    }

    public interface IPageCollection
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        string Title { get; }
        string PageTypeName { get; }
        Guid? PageId { get; }
        PageSortMode SortMode { get; }
    }
}