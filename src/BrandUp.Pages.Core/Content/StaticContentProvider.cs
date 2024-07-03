using BrandUp.Pages.Content.Items;

namespace BrandUp.Pages.Content
{
    public class StaticContentProvider : ItemContentProvider<StaticContent>
    {
        #region ItemContentProvider members

        public override async Task<string> GetContentKeyAsync(StaticContent item, CancellationToken cancellationToken)
        {
            return await Task.FromResult(item.Key);
        }

        public override async Task<Type> GetContentTypeAsync(StaticContent item, CancellationToken cancellationToken)
        {
            return await Task.FromResult(item.Type);
        }

        #endregion
    }

    [ContentItem("brandup.content-static")]
    public readonly struct StaticContent(string key, Type type) : IItemContent
    {
        public string Key { get; } = key ?? throw new ArgumentNullException(nameof(key));
        public Type Type { get; } = type ?? throw new ArgumentNullException(nameof(type));

        public string ItemId => Key;
    }
}