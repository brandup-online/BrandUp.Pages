namespace BrandUp.Pages.Interfaces
{
	public interface IPageContentRepository
	{
		Task<IPageEdit> CreateEditAsync(IPage page, string userId, CancellationToken cancellationToken = default);
		Task<IPageEdit> FindEditByUserAsync(IPage page, string userId, CancellationToken cancellationToken = default);
		Task<IPageEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default);
		Task<IDictionary<string, object>> GetContentAsync(IPageEdit pageEdit, CancellationToken cancellationToken = default);
		Task SetContentAsync(IPageEdit pageEdit, IDictionary<string, object> contentData, CancellationToken cancellationToken = default);
		Task DeleteEditAsync(IPageEdit pageEdit, CancellationToken cancellationToken = default);
	}
}