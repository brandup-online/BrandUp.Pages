using BrandUp.Pages.Builder;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Helpers;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Website;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Services
{
    public class PageCollectionServiceTests : IAsyncLifetime
    {
        readonly ServiceProvider serviceProvider;
        readonly IServiceScope serviceScope;
        readonly PageService pageService;
        readonly PageCollectionService pageCollectionService;
        readonly PageMetadataManager pageMetadataManager;
        readonly IWebsiteContext websiteContext;

        public PageCollectionServiceTests()
        {
            websiteContext = new TestWebsiteContext("test", "test");

            var services = new ServiceCollection();

            services.AddPages()
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakes();

            services.AddSingleton(websiteContext);

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageService = serviceScope.ServiceProvider.GetService<PageService>();
            pageCollectionService = serviceScope.ServiceProvider.GetService<PageCollectionService>();
            pageMetadataManager = serviceScope.ServiceProvider.GetService<PageMetadataManager>();
        }

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            var pageCollectionRepository = serviceScope.ServiceProvider.GetService<IPageCollectionRepository>();
            var pageRepository = serviceScope.ServiceProvider.GetService<IPageRepository>();

            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var pageCollection = await pageCollectionRepository.CreateCollectionAsync("test", "Test collection", pageType.Name, PageSortMode.FirstOld, null);

            var pageId = Guid.NewGuid();
            var mainPage = await pageRepository.CreatePageAsync("test", pageCollection.Id, pageId, pageType.Name, "test");
            await pageRepository.SetUrlPathAsync(mainPage, "index");
            await pageRepository.UpdatePageAsync(mainPage);

            pageId = Guid.NewGuid();
            var testPage = await pageRepository.CreatePageAsync("test", pageCollection.Id, pageId, pageType.Name, "test");
            await pageRepository.SetUrlPathAsync(testPage, "test");
            await pageRepository.UpdatePageAsync(testPage);
        }
        async Task IAsyncLifetime.DisposeAsync()
        {
            serviceScope.Dispose();
            await serviceProvider.DisposeAsync();
        }

        #endregion

        #region Test methods

        [Fact]
        public async Task CreateCollection_root()
        {
            var result = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "Test collection", "TestPage", PageSortMode.FirstOld);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Test collection", result.Data.Title);
            Assert.Equal("TestPage", result.Data.PageTypeName);
            Assert.Null(result.Data.PageId);
            Assert.Equal(PageSortMode.FirstOld, result.Data.SortMode);
        }

        [Fact]
        public async Task CreateCollection_bypage()
        {
            var defaultPage = await pageService.GetDefaultPageAsync(websiteContext.Website.Id);
            var result = await pageCollectionService.CreateCollectionAsync(defaultPage, "Test collection", "TestPage", PageSortMode.FirstOld);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Test collection", result.Data.Title);
            Assert.Equal("TestPage", result.Data.PageTypeName);
            Assert.Equal(defaultPage.Id, result.Data.PageId);
            Assert.Equal(PageSortMode.FirstOld, result.Data.SortMode);
        }

        [Fact]
        public async Task CreateCollection_Fail_PageNotPublished()
        {
            var parentPageCollection = (await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id)).First();
            var page = await pageService.CreatePageAsync(parentPageCollection);

            var pageCollection = await pageCollectionService.CreateCollectionAsync(page, "Test collection", "TestPage", PageSortMode.FirstOld);

            Assert.False(pageCollection.IsSuccess);
        }

        [Fact]
        public async Task FindCollectiondById()
        {
            var pageCollection = (await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "Test collection", "TestPage", PageSortMode.FirstOld)).Data;

            var findedPageCollection = await pageCollectionService.FindCollectiondByIdAsync(pageCollection.Id);

            Assert.NotNull(findedPageCollection);
            Assert.Equal(pageCollection.Id, findedPageCollection.Id);
        }

        [Fact]
        public async Task UpdateCollection()
        {
            var pageCollection = (await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "Test collection", "TestPage", PageSortMode.FirstOld)).Data;

            pageCollection.SetTitle("New title");
            pageCollection.SetSortModel(PageSortMode.FirstNew);

            var result = await pageCollectionService.UpdateCollectionAsync(pageCollection);

            Assert.True(result.IsSuccess);
            Assert.Equal("New title", pageCollection.Title);
            Assert.Equal(PageSortMode.FirstNew, pageCollection.SortMode);
        }

        [Fact]
        public async Task DeleteCollection()
        {
            var pageCollection = (await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "Test collection", "TestPage", PageSortMode.FirstOld)).Data;

            var result = await pageCollectionService.DeleteCollectionAsync(pageCollection);

            Assert.True(result.IsSuccess);
            Assert.Null(await pageCollectionService.FindCollectiondByIdAsync(pageCollection.Id));
        }

        [Fact]
        public async Task DeleteCollection_Fail_HavePages()
        {
            var pageCollection = (await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id)).First();

            var result = await pageCollectionService.DeleteCollectionAsync(pageCollection);

            Assert.False(result.IsSuccess);
        }

        #endregion
    }
}