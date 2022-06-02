using BrandUp.Extensions.Migrations;
using System.Threading;
using System.Threading.Tasks;

namespace LandingWebSite._migrations
{
    [Setup]
    public class SetupMigration : IMigrationHandler
    {
        //private readonly IPageCollectionService pageCollectionService;
        //private readonly IPageMetadataManager pageMetadataManager;
        //private readonly IPageService pageService;

        //public SetupMigration(IPageCollectionService pageCollectionService, IPageMetadataManager pageMetadataManager, IPageService pageService)
        //{
        //    this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
        //    this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
        //    this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        //}

        public Task UpAsync(CancellationToken cancellationToken = default)
        {
            //var commonPageMetadata = pageMetadataManager.FindPageMetadataByContentType(typeof(Contents.Page.CommonPageContent));
            //var newsPageMetadata = pageMetadataManager.FindPageMetadataByContentType(typeof(Contents.Page.NewsPageContent));

            //var mainPages = (await pageCollectionService.CreateCollectionAsync("Main pages", commonPageMetadata.Name, PageSortMode.FirstOld, null)).Data;

            //var homePage = await pageService.CreatePageAsync(mainPages, new Contents.Page.CommonPageContent { Header = "Home page" });
            //await pageService.PublishPageAsync(homePage, "index");

            //var newsListPages = await pageService.CreatePageAsync(mainPages, new Contents.Page.NewsListPageContent { Header = "News" });
            //await pageService.PublishPageAsync(newsListPages, "news");

            //var newsPages = (await pageCollectionService.CreateCollectionAsync("Actual news", newsPageMetadata.Name, PageSortMode.FirstNew, newsListPages.Id)).Data;
            //await pageCollectionService.CreateCollectionAsync("Archive news", newsPageMetadata.Name, PageSortMode.FirstNew, newsListPages.Id);

            //var newsPage1 = await pageService.CreatePageAsync(newsPages, new Contents.Page.NewsPageContent { Header = "First news", SubHeader = "This is first news" });
            //await pageService.PublishPageAsync(newsPage1, "first-news");

            //var newsPage2 = await pageService.CreatePageAsync(newsPages, new Contents.Page.NewsPageContent { Header = "Second news", SubHeader = "This is second news" });
            //await pageService.PublishPageAsync(newsPage2, "second-news");

            return Task.CompletedTask;
        }

        public Task DownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}