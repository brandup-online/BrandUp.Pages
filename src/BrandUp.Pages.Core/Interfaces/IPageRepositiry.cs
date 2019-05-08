using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageRepositiry
    {
        Task<IPage> CreatePageAsync(Guid сollectionId, string typeName, string title, IDictionary<string, object> contentData);
        Task<IPage> FindPageByIdAsync(Guid id);
        Task<IPage> FindPageByPathAsync(string path);
        Task<IEnumerable<IPage>> GetPagesAsync(Guid сollectionId, PageSortMode pageSort, PagePaginationOptions pagination);
        Task<bool> HasPagesAsync(Guid сollectionId);
        Task<PageContent> GetContentAsync(Guid pageId);
        Task SetContentAsync(Guid pageId, string title, PageContent content);
        Task SetUrlPathAsync(Guid pageId, string urlPath);
        Task DeletePageAsync(Guid pageId);
    }

    public class PageContent
    {
        public int Version { get; }
        public IDictionary<string, object> Data { get; }

        public PageContent(int version, IDictionary<string, object> data)
        {
            if (version < 1)
                throw new ArgumentOutOfRangeException(nameof(version));

            Version = version;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }

    public class PagePaginationOptions
    {
        public int Skip { get; }
        public int Limit { get; }

        public PagePaginationOptions(int skip, int limit)
        {
            if (skip < 0)
                throw new ArgumentOutOfRangeException(nameof(skip));
            if (limit <= skip || limit - skip > 50)
                throw new ArgumentOutOfRangeException(nameof(limit));

            Skip = skip;
            Limit = limit;
        }
        public PagePaginationOptions(int limit) : this(0, limit) { }
    }
}