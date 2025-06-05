using BrandUp.Pages.Content.Items;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Services;

namespace BrandUp.Pages.Content
{
    public class PageContentProvider(PageMetadataManager pageMetadataManager, PageService pageService) : ItemContentProvider<IPage>
    {
        #region ItemContentProvider members

        public override async Task<string> GetContentKeyAsync(IPage item, CancellationToken cancellationToken)
        {
            return await Task.FromResult($"{item.WebsiteId}-page-{item.ItemId}");
        }

        public override async Task OnDefaultFactoryAsync(string itemId, object content, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(itemId, out var pageId))
                throw new FormatException("Unable parse page ID as Guid.");

            var page = await pageService.FindPageByIdAsync(pageId, cancellationToken);

            var pageProvider = pageMetadataManager.GetMetadata(page.TypeName);

            pageProvider.SetPageHeader(content, page.Header);

            await Task.CompletedTask;
        }

        public override async Task OnUpdatedContentAsync(string itemId, object content, CancellationToken cancellationToken)
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