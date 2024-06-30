using BrandUp.Pages.Content.Items;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Services;

namespace BrandUp.Pages.Content
{
    public class PageContentProvider(PageMetadataManager pageMetadataManager) : IItemContentProvider<IPage>
    {
        #region IItemMappingProvider members

        public async Task<string> GetContentKeyAsync(IPage item, CancellationToken cancellationToken)
        {
            return await Task.FromResult($"page-{item.ItemId}");
        }

        public async Task<Type> GetContentTypeAsync(IPage item, CancellationToken cancellationToken)
        {
            var pageProvider = pageMetadataManager.GetMetadata(item.TypeName);

            return await Task.FromResult(pageProvider.ContentType);
        }

        public Task OnUpdatedContentAsync(string itemId, object content, CancellationToken cancellationToken)
        {
            var pageProvider = pageMetadataManager.GetMetadata(content.GetType());

            throw new NotImplementedException();
        }

        #endregion
    }
}