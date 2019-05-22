using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageRepositiry
    {
        Task<IPage> CreatePageAsync(Guid сollectionId, string typeName, string title, IDictionary<string, object> contentData);
        Task<IPage> FindPageByIdAsync(Guid id);
        Task<IPage> FindPageByPathAsync(string path);
        Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> SearchPagesAsync(string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default);
        Task<bool> HasPagesAsync(Guid сollectionId);
        Task<IDictionary<string, object>> GetContentAsync(Guid pageId);
        Task SetContentAsync(Guid pageId, string title, IDictionary<string, object> contentData);
        Task UpdatePageAsync(IPage page, CancellationToken cancellationToken = default);
        Task DeletePageAsync(IPage page, CancellationToken cancellationToken = default);
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