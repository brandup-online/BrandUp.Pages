namespace BrandUp.Pages.Items
{
    public interface IItemProvider<TItem>
        where TItem : class
    {
        Task<string> GetIdAsync(TItem item, CancellationToken cancellationToken = default);
        Task<TItem> FindByIdAsync(string id, CancellationToken cancellationToken = default);
    }

    public interface IPageCallbacks
    {
        Task UpdateHeaderAsync(string id, string header, CancellationToken cancellationToken = default);
    }
}