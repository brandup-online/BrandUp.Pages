using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageCollectionRepositiry
    {
        Task<IPageCollection> CreateCollectionAsync(string title, string pageTypeName, PageSortMode sortMode, Guid? pageId);
        Task<IPageCollection> FindCollectiondByIdAsync(Guid id);
        Task<IEnumerable<IPageCollection>> GetCollectionsAsync(Guid? pageId);
        Task<IEnumerable<IPageCollection>> GetCollectionsAsync(string[] pageTypeNames, string title);
        Task<IPageCollection> UpdateCollectionAsync(Guid id, string title, PageSortMode pageSort);
        Task DeleteCollectionAsync(Guid id);
    }
}