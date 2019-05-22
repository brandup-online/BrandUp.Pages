using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageRepository
    {
        Task<IPage> CreatePageAsync(Guid сollectionId, string typeName, string title, IDictionary<string, object> contentData);
        Task<IPage> FindPageByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IPage> FindPageByPathAsync(string path, CancellationToken cancellationToken = default);
        Task<PageUrlResult> FindPageUrlAsync(string path, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default);
        Task<IEnumerable<IPage>> SearchPagesAsync(string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default);
        Task<bool> HasPagesAsync(Guid сollectionId, CancellationToken cancellationToken = default);
        Task<IDictionary<string, object>> GetContentAsync(Guid pageId, CancellationToken cancellationToken = default);
        Task SetContentAsync(Guid pageId, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
        Task UpdatePageAsync(IPage page, CancellationToken cancellationToken = default);
        Task DeletePageAsync(IPage page, CancellationToken cancellationToken = default);
    }

    public class PageUrlResult
    {
        public Guid? PageId { get; }
        public PageUrlRedirect Redirect { get; }

        public PageUrlResult(Guid pageId)
        {
            PageId = pageId;
        }
        public PageUrlResult(PageUrlRedirect redirect)
        {
            Redirect = redirect;
        }
    }

    public class PageUrlRedirect
    {
        public string Path { get; }
        public bool IsPermament { get; }

        public PageUrlRedirect(string path, bool isPermament)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            IsPermament = isPermament;
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