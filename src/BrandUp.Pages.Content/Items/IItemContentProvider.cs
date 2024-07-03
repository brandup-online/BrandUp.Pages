namespace BrandUp.Pages.Content.Items
{
    public interface IItemContentProvider : IItemContentEvents
    {
        Task<string> GetContentKeyAsync(object item, CancellationToken cancellationToken);
        Task<Type> GetContentTypeAsync(object item, CancellationToken cancellationToken);
    }

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

        Task<Type> IItemContentProvider.GetContentTypeAsync(object item, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(item);

            var typedItem = (TItem)item;

            return GetContentTypeAsync(typedItem, cancellationToken);
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
        public abstract Task<Type> GetContentTypeAsync(TItem item, CancellationToken cancellationToken);
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