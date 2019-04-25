﻿using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageService
    {
        Task<IPage> CreatePageAsync(IPageCollection collection, string pageType);
        Task<IPage> FindPageByIdAsync(Guid id);
        Task<IPage> FindPageByPathAsync(string pagePath);
        Task<IPage> GetDefaultPageAsync();
        Task SetDefaultPageAsync(IPage page);
        Task<IEnumerable<IPage>> GetPagesAsync(IPageCollection collection, PagePaginationOptions pagination);
        Task<PageMetadataProvider> GetPageTypeAsync(IPage page);
        Task<object> GetPageContentAsync(IPage page);
        Task SetPageContentAsync(IPage page, object contentModel);
        Task PublishPageAsync(IPage page, string urlPathName);
        Task<Result> DeletePageAsync(IPage page);
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