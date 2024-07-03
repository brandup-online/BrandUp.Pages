namespace BrandUp.Pages.Content.Items
{
    public abstract class ItemContentProvider<TItem> : IItemContentProvider
        where TItem : IItemContent
    {
        #region IItemContentProvider members

        Task<string> IItemContentProvider.GetContentKeyAsync(object item, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(item);

            var typedItem = (TItem)item;

            return GetContentKeyAsync(typedItem, cancellationToken);
        }

        public virtual Task OnDefaultFactoryAsync(string itemId, object content, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnUpdatedContentAsync(string itemId, object content, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion

        public abstract Task<string> GetContentKeyAsync(TItem item, CancellationToken cancellationToken);
    }

    public interface IItemContentProvider : IItemContentEvents
    {
        Task<string> GetContentKeyAsync(object item, CancellationToken cancellationToken);
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