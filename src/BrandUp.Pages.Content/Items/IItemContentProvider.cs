namespace BrandUp.Pages.Content.Items
{
    public interface IItemContentProvider<TItem>
        where TItem : IItemContent
    {
        Task<string> GetContentKeyAsync(TItem item, CancellationToken cancellationToken);
        Task<Type> GetContentTypeAsync(TItem item, CancellationToken cancellationToken);
        Task DefaultFactoryAsync(TItem item, object content, CancellationToken cancellationToken);
        Task OnUpdatedContentAsync(string itemId, object content, CancellationToken cancellationToken);
    }

    public interface IItemContent
    {
        string ItemId { get; }
    }
}