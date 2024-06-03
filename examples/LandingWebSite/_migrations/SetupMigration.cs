using BrandUp.Extensions.Migrations;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Website;

namespace LandingWebSite._migrations
{
    [Setup]
    public class SetupMigration : IMigrationHandler
    {
        readonly IPageCollectionService pageCollectionService;
        readonly IPageMetadataManager pageMetadataManager;
        readonly IPageService pageService;
        readonly IWebsiteStore websiteStore;

        public SetupMigration(IPageCollectionService pageCollectionService, IPageMetadataManager pageMetadataManager, IPageService pageService, IWebsiteStore websiteStore)
        {
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.websiteStore = websiteStore ?? throw new ArgumentNullException(nameof(websiteStore));
        }

        public async Task UpAsync(CancellationToken cancellationToken = default)
        {
            var website = await websiteStore.FindByNameAsync(string.Empty);

            var commonPageMetadata = pageMetadataManager.FindPageMetadataByContentType(typeof(Contents.Page.CommonPageContent));
            var newsPageMetadata = pageMetadataManager.FindPageMetadataByContentType(typeof(Contents.Page.NewsPageContent));

            var mainPages = (await pageCollectionService.CreateCollectionAsync(website.Id, "Main pages", commonPageMetadata.Name, PageSortMode.FirstOld)).Data;

            var homePage = await pageService.CreatePageAsync(mainPages, new Contents.Page.CommonPageContent { Header = "Home page" }, cancellationToken);
            await pageService.PublishPageAsync(homePage, "index", cancellationToken);

            var newsListPages = await pageService.CreatePageAsync(mainPages, new Contents.Page.NewsListPageContent { Header = "News" }, cancellationToken);
            await pageService.PublishPageAsync(newsListPages, "news", cancellationToken);

            var newsPages = (await pageCollectionService.CreateCollectionAsync(newsListPages, "Actual news", newsPageMetadata.Name, PageSortMode.FirstNew)).Data;
            await pageCollectionService.CreateCollectionAsync(newsListPages, "Archive news", newsPageMetadata.Name, PageSortMode.FirstNew);

            var newsPage1 = await pageService.CreatePageAsync(newsPages, new Contents.Page.NewsPageContent { Header = "First news", SubHeader = "This is first news" }, cancellationToken);
            await pageService.PublishPageAsync(newsPage1, "first-news", cancellationToken);

            var newsPage2 = await pageService.CreatePageAsync(newsPages, new Contents.Page.NewsPageContent { Header = "Second news", SubHeader = "This is second news" }, cancellationToken);
            await pageService.PublishPageAsync(newsPage2, "second-news", cancellationToken);
        }

        public Task DownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}