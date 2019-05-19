using BrandUp.Extensions.Migrations;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LandingWebSite._migrations
{
    [Migration("1.0.0", "CreateBasePages")]
    public class Migration1 : IMigration
    {
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageMetadataManager pageMetadataManager;
        private readonly IPageService pageService;

        public Migration1(IPageCollectionService pageCollectionService, IPageMetadataManager pageMetadataManager, IPageService pageService)
        {
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        public async Task UpAsync(CancellationToken cancellationToken = default)
        {
            var pageContentMetadata = pageMetadataManager.FindPageMetadataByContentType(typeof(Contents.Page.CommonPageContent));

            var pageCollection = await pageCollectionService.CreateCollectionAsync("Main pages", pageContentMetadata.Name, PageSortMode.FirstOld, null);

            var page = await pageService.CreatePageAsync(pageCollection, pageContentMetadata.Name);

            await pageService.PublishPageAsync(page, "index");
        }

        public Task DownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}