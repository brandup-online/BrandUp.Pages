using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageService : IDisposable
    {
        Task<IPage> CreatePageAsync(IPageCollection collection);
        Task<IPage> FindPageByIdAsync(Guid id);
        Task<IPage> FindPageByPathAsync(string pagePath);
        Task<IPage> GetDefaultPageAsync();
        Task<IEnumerable<IPage>> GetPagesAsync(IPageCollection collection, PagePaginationOptions pagination);
        Task<PageMetadataProvider> GetPageTypeAsync(IPage page);
        Task<object> GetPageContentAsync(IPage page);
        Task PublishPageAsync(IPage page, string urlPathName);
        Task DeletePageAsync(IPage page);
    }

    public interface IPage
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        string TypeName { get; }
        Guid OwnCollectionId { get; }
        string UrlPath { get; }
        int ContentVersion { get; }
    }

    public enum PageSortMode
    {
        FirstOld = 0,
        FirstNew = 1
    }
}