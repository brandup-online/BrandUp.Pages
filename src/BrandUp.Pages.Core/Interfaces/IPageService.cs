using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageService
    {
        Task<IPage> CreatePageAsync(IPageCollection collection, object pageContent, CancellationToken cancellationToken = default);
        Task<IPage> CreatePageAsync(IPageCollection collection, string pageType = null, string pageHeader = null, CancellationToken cancellationToken = default);
        Task<IPage> FindPageByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IPage> FindPageByPathAsync(string pagePath, CancellationToken cancellationToken = default);
        Task<PageUrlResult> FindPageUrlAsync(string path, CancellationToken cancellationToken = default);
        Task<IPage> GetDefaultPageAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> SearchPagesAsync(string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default);
        Task<PageMetadataProvider> GetPageTypeAsync(IPage page, CancellationToken cancellationToken = default);
        Task<object> GetPageContentAsync(IPage page, CancellationToken cancellationToken = default);
        Task SetPageContentAsync(IPage page, object contentModel, CancellationToken cancellationToken = default);
        Task<Result> PublishPageAsync(IPage page, string urlPath, CancellationToken cancellationToken = default);
        Task<Result> DeletePageAsync(IPage page, CancellationToken cancellationToken = default);
        Task<Guid?> GetParentPageIdAsync(IPage page, CancellationToken cancellationToken = default);
    }

    public interface IPage
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        string TypeName { get; }
        Guid OwnCollectionId { get; }
        string Header { get; set; }
        string UrlPath { get; }
        bool IsPublished { get; }

        Task SetUrlAsync(string urlPath);
    }

    public enum PageSortMode
    {
        FirstOld = 0,
        FirstNew = 1
    }

    public class GetPagesOptions
    {
        public Guid CollectionId { get; set; }
        public PageSortMode? Sorting { get; set; }
        public bool IncludeDrafts { get; set; }
        public PagePaginationOptions Pagination { get; set; }

        public GetPagesOptions(Guid collectionId)
        {
            CollectionId = collectionId;
        }
    }
}