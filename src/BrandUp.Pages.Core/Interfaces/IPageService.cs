using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageService
    {
        Task<IPage> CreatePageAsync(IPageCollection collection, string pageType = null, string pageTitle = null);
        Task<IPage> FindPageByIdAsync(Guid id);
        Task<IPage> FindPageByPathAsync(string pagePath);
        Task<IPage> GetDefaultPageAsync();
        Task<IEnumerable<IPage>> GetPagesAsync(IPageCollection collection, PagePaginationOptions pagination);
        Task<IEnumerable<IPage>> SearchPagesAsync(string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default);
        Task<PageMetadataProvider> GetPageTypeAsync(IPage page);
        Task<object> GetPageContentAsync(IPage page);
        Task SetPageContentAsync(IPage page, object contentModel);
        Task<Result> PublishPageAsync(IPage page, string urlPath, CancellationToken cancellationToken = default);
        Task<Result> DeletePageAsync(IPage page, CancellationToken cancellationToken = default);
        Task<Guid?> GetParentPageIdAsync(IPage page);
    }

    public interface IPage
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        string TypeName { get; }
        Guid OwnCollectionId { get; }
        string Title { get; set; }
        string UrlPath { get; }
        bool IsPublished { get; }

        Task SetUrlAsync(string urlPath);
    }

    public enum PageSortMode
    {
        FirstOld = 0,
        FirstNew = 1
    }
}