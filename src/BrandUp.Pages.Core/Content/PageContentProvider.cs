using BrandUp.Pages.Content.Items;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Services;

namespace BrandUp.Pages.Content
{
    public class PageContentProvider(PageMetadataManager pageMetadataManager, PageService pageService) : IItemContentProvider<IPage>
    {
        #region IItemMappingProvider members

        public async Task<string> GetContentKeyAsync(IPage item, CancellationToken cancellationToken)
        {
            return await Task.FromResult($"{item.WebsiteId}-page-{item.ItemId}");
        }

        public async Task<Type> GetContentTypeAsync(IPage item, CancellationToken cancellationToken)
        {
            var pageProvider = pageMetadataManager.GetMetadata(item.TypeName);

            return await Task.FromResult(pageProvider.ContentType);
        }

        public async Task DefaultFactoryAsync(IPage item, object content, CancellationToken cancellationToken)
        {
            var pageProvider = pageMetadataManager.GetMetadata(item.TypeName);

            pageProvider.SetPageHeader(content, item.Header);

            await Task.CompletedTask;
        }

        public async Task OnUpdatedContentAsync(string itemId, object content, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(itemId, out var pageId))
                throw new FormatException("Unable parse page ID as Guid.");

            var pageProvider = pageMetadataManager.GetMetadata(content.GetType());
            var pageHeader = pageProvider.GetPageHeader(content);

            var page = await pageService.FindPageByIdAsync(pageId, cancellationToken);
            if (page == null)
                throw new InvalidOperationException("Not found page by ID.");

            var updateResult = await pageService.UpdateHeaderAsync(page, pageHeader, cancellationToken);
            if (!updateResult)
                throw new InvalidOperationException("Unable to update page data.");
        }

        #endregion
    }
}