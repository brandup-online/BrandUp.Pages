using BrandUp.Pages.Metadata;

namespace BrandUp.Pages
{
    public interface IPageCollectionService
    {
        /// <summary>
        /// Создание корневой коллекции. 
        /// </summary>
        Task<Result<IPageCollection>> CreateCollectionAsync(string webSiteId, string title, string pageTypeName, PageSortMode sortMode);
        /// <summary>
        /// Создание коллекции страниц для страницы.
        /// </summary>
        Task<Result<IPageCollection>> CreateCollectionAsync(IPage page, string title, string pageTypeName, PageSortMode sortMode);
        Task<IPageCollection> FindCollectiondByIdAsync(Guid id);
        Task<IEnumerable<IPageCollection>> ListCollectionsAsync(string webSiteId);
        Task<IEnumerable<IPageCollection>> ListCollectionsAsync(IPage page);
        Task<IEnumerable<IPageCollection>> FindCollectionsAsync(string webSiteId, string pageTypeName, string title = null, bool includeDerivedTypes = true);
        Task<Result> UpdateCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default);
        Task<Result> DeleteCollectionAsync(IPageCollection collection, CancellationToken cancellationToken = default);
        Task<List<PageMetadataProvider>> GetPageTypesAsync(IPageCollection collection);
    }

    public interface IPageCollection
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        string WebsiteId { get; }
        string Title { get; }
        string PageTypeName { get; }
        Guid? PageId { get; }
        PageSortMode SortMode { get; }
        bool CustomSorting { get; }

        void SetTitle(string newTitle);
        void SetSortModel(PageSortMode sortMode);
        void SetCustomSorting(bool enabledCustomSorting);
    }
}