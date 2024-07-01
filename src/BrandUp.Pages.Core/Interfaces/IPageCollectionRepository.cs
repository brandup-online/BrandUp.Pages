using BrandUp.Pages.Services;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageCollectionRepository
    {
        Task<IPageCollection> CreateCollectionAsync(string webSiteId, string title, string pageTypeName, PageSortMode sortMode, Guid? pageId = null);
        Task<IPageCollection> FindCollectiondByIdAsync(Guid id);
        Task<IEnumerable<IPageCollection>> ListCollectionsAsync(string webSiteId, Guid? pageId = null);
        Task<IEnumerable<IPageCollection>> FindCollectionsAsync(string webSiteId, string[] pageTypeNames, string title = null);
        Task UpdateCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default);
        Task DeleteCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default);
    }
}