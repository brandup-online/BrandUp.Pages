namespace BrandUp.Pages.Interfaces
{
	public interface IPageRepository
	{
		Task<IPage> CreatePageAsync(string websiteId, Guid сollectionId, string typeName, string pageHeader, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
		Task<IPage> FindPageByIdAsync(Guid id, CancellationToken cancellationToken = default);
		Task<IPage> FindPageByPathAsync(string websiteId, string path, CancellationToken cancellationToken = default);
		Task<PageUrlResult> FindUrlByPathAsync(string websiteId, string path, CancellationToken cancellationToken = default);
		Task<IEnumerable<IPage>> GetPagesAsync(GetPagesOptions options, CancellationToken cancellationToken = default);
		Task<IEnumerable<IPage>> GetPublishedPagesAsync(string websiteId, CancellationToken cancellationToken = default);
		Task<IEnumerable<IPage>> SearchPagesAsync(string webSiteId, string title, PagePaginationOptions pagination, CancellationToken cancellationToken = default);
		Task<bool> HasPagesAsync(Guid сollectionId, CancellationToken cancellationToken = default);
		Task<IDictionary<string, object>> GetContentAsync(Guid pageId, CancellationToken cancellationToken = default);
		Task SetContentAsync(Guid pageId, string title, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
		Task UpdatePageAsync(IPage page, CancellationToken cancellationToken = default);
		Task DeletePageAsync(IPage page, CancellationToken cancellationToken = default);
		Task SetUrlPathAsync(IPage page, string urlPath, CancellationToken cancellationToken = default);
		Task<string> GetPageTitleAsync(IPage page, CancellationToken cancellationToken = default);
		Task SetPageTitleAsync(IPage page, string title, CancellationToken cancellationToken = default);
		Task<string> GetPageDescriptionAsync(IPage page, CancellationToken cancellationToken = default);
		Task SetPageDescriptionAsync(IPage page, string description, CancellationToken cancellationToken = default);
		Task<string[]> GetPageKeywordsAsync(IPage page, CancellationToken cancellationToken = default);
		Task SetPageKeywordsAsync(IPage page, string[] keywords, CancellationToken cancellationToken = default);
		Task UpPagePositionAsync(IPage page, IPage beforePage, CancellationToken cancellationToken = default);
		Task DownPagePositionAsync(IPage page, IPage afterPage, CancellationToken cancellationToken = default);
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