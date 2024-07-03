namespace BrandUp.Pages.Content.Items
{
    public interface IItemContentProvider<TItem> : IItemContentEvents
        where TItem : IItemContent
    {
        Task<string> GetContentKeyAsync(TItem item, CancellationToken cancellationToken);
        Task<Type> GetContentTypeAsync(TItem item, CancellationToken cancellationToken);
    }

    public interface IItemContentEvents
    {
        Task OnDefaultFactoryAsync(string itemId, object content, CancellationToken cancellationToken);
        Task OnUpdatedContentAsync(string itemId, object content, CancellationToken cancellationToken);
    }

    public interface IItemContent
    {
        string ItemId { get; }
    }
}