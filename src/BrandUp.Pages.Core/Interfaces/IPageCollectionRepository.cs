using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageCollectionRepository
    {
        Task<IPageCollection> CreateCollectionAsync(string webSiteId, string title, string pageTypeName, PageSortMode sortMode, Guid? pageId);
        Task<IPageCollection> FindCollectiondByIdAsync(Guid id);
        Task<IEnumerable<IPageCollection>> GetCollectionsAsync(string webSiteId, Guid? pageId);
        Task<IEnumerable<IPageCollection>> GetCollectionsAsync(string webSiteId, string[] pageTypeNames, string title);
        Task UpdateCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default);
        Task DeleteCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default);
    }
}