using BrandUp.Pages.Content.Items;

namespace BrandUp.Pages.Content
{
    public class StaticContentProvider : IItemContentProvider<StaticContent>
    {
        public async Task<string> GetContentKeyAsync(StaticContent item, CancellationToken cancellationToken)
        {
            return await Task.FromResult(item.Key);
        }

        public async Task<Type> GetContentTypeAsync(StaticContent item, CancellationToken cancellationToken)
        {
            return await Task.FromResult(item.Type);
        }

        public async Task OnDefaultFactoryAsync(string itemId, object content, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task OnUpdatedContentAsync(string itemId, object content, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }

    [ContentItem("brandup.content-static")]
    public readonly struct StaticContent(string key, Type type) : IItemContent
    {
        public string Key { get; } = key ?? throw new ArgumentNullException(nameof(key));
        public Type Type { get; } = type ?? throw new ArgumentNullException(nameof(type));

        string IItemContent.ItemId => Key;
    }
}