using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LandingWebSite.Controllers
{
    [Route("_setup")]
    public class SetupController : Controller
    {
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageMetadataManager pageMetadataManager;
        private readonly IPageService pageService;

        public SetupController(IPageCollectionService pageCollectionService, IPageMetadataManager pageMetadataManager, IPageService pageService)
        {
            this.pageCollectionService = pageCollectionService ?? throw new ArgumentNullException(nameof(pageCollectionService));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        [HttpGet("pages")]
        public async Task<IActionResult> CreatePages()
        {
            var commonPageMetadata = pageMetadataManager.FindPageMetadataByContentType(typeof(Contents.Page.CommonPageContent));
            var newsPageMetadata = pageMetadataManager.FindPageMetadataByContentType(typeof(Contents.Page.NewsPageContent));

            var mainPages = (await pageCollectionService.CreateCollectionAsync("Main pages", commonPageMetadata.Name, PageSortMode.FirstOld, null)).Data;

            var homePage = await pageService.CreatePageAsync(mainPages, new Contents.Page.CommonPageContent { Header = "Home page" });
            await pageService.PublishPageAsync(homePage, "index");

            var newsListPages = await pageService.CreatePageAsync(mainPages, new Contents.Page.NewsListPageContent { Header = "News" });
            await pageService.PublishPageAsync(newsListPages, "news");

            var newsPages = (await pageCollectionService.CreateCollectionAsync("Actual news", newsPageMetadata.Name, PageSortMode.FirstNew, newsListPages.Id)).Data;
            await pageCollectionService.CreateCollectionAsync("Archive news", newsPageMetadata.Name, PageSortMode.FirstNew, newsListPages.Id);

            var newsPage1 = await pageService.CreatePageAsync(newsPages, new Contents.Page.NewsPageContent { Header = "First news", SubHeader = "This is first news" });
            await pageService.PublishPageAsync(newsPage1, "first-news");

            var newsPage2 = await pageService.CreatePageAsync(newsPages, new Contents.Page.NewsPageContent { Header = "Second news", SubHeader = "This is second news" });
            await pageService.PublishPageAsync(newsPage2, "second-news");

            return Ok("ok");
        }
    }
}