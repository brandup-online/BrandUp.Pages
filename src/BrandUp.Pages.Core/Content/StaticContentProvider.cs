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

        #endregion
    }

    [ContentItem("brandup.content-static")]
    public readonly struct StaticContent(string key) : IItemContent
    {
        public string Key { get; } = key ?? throw new ArgumentNullException(nameof(key));

        public string ItemId => Key;
    }
}