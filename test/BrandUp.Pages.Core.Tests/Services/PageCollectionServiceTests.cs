using BrandUp.Pages.Builder;
using BrandUp.Pages.Content;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.Services
{
    public class PageCollectionServiceTests : IAsyncLifetime
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private readonly IPageService pageService;
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageMetadataManager pageMetadataManager;

        public PageCollectionServiceTests()
        {
            var services = new ServiceCollection();

            services.AddPages()
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakeRepositories();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageService = serviceScope.ServiceProvider.GetService<IPageService>();
            pageCollectionService = serviceScope.ServiceProvider.GetService<IPageCollectionService>();
            pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
        }

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            var contentMetadataManager = serviceScope.ServiceProvider.GetService<IContentMetadataManager>();
            var pageCollectionRepository = serviceScope.ServiceProvider.GetService<IPageCollectionRepositiry>();
            var pageRepository = serviceScope.ServiceProvider.GetService<IPageRepositiry>();

            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var pageCollection = await pageCollectionRepository.CreateCollectionAsync("Test collection", pageType.Name, PageSortMode.FirstOld, null);

            var mainPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, "test", pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await mainPage.SetUrlAsync("index");
            await pageRepository.UpdatePageAsync(mainPage);

            var testPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, "test", pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await testPage.SetUrlAsync("test");
            await pageRepository.UpdatePageAsync(testPage);
        }
        Task IAsyncLifetime.DisposeAsync()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();

            return Task.CompletedTask;
        }

        #endregion

        #region Test methods

        [Fact]
        public async Task CreateCollection_root()
        {
            var pageCollection = await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, null);

            Assert.NotNull(pageCollection);
            Assert.Equal("Test collection", pageCollection.Title);
            Assert.Equal("TestPage", pageCollection.PageTypeName);
            Assert.Null(pageCollection.PageId);
            Assert.Equal(PageSortMode.FirstOld, pageCollection.SortMode);
        }

        [Fact]
        public async Task CreateCollection_bypage()
        {
            var defaultPage = await pageService.GetDefaultPageAsync();
            var pageCollection = await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, defaultPage.Id);

            Assert.NotNull(pageCollection);
            Assert.Equal("Test collection", pageCollection.Title);
            Assert.Equal("TestPage", pageCollection.PageTypeName);
            Assert.Equal(defaultPage.Id, pageCollection.PageId);
            Assert.Equal(PageSortMode.FirstOld, pageCollection.SortMode);
        }

        [Fact]
        public async Task FindCollectiondById()
        {
            var pageCollection = await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, null);

            var findedPageCollection = await pageCollectionService.FindCollectiondByIdAsync(pageCollection.Id);

            Assert.NotNull(findedPageCollection);
            Assert.Equal(pageCollection.Id, findedPageCollection.Id);
        }

        [Fact]
        public async Task UpdateCollection()
        {
            var pageCollection = await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, null);

            pageCollection.SetTitle("New title");
            pageCollection.SetSortModel(PageSortMode.FirstNew);

            await pageCollectionService.UpdateCollectionAsync(pageCollection);

            Assert.Equal("New title", pageCollection.Title);
            Assert.Equal(PageSortMode.FirstNew, pageCollection.SortMode);
        }

        [Fact]
        public async Task DeleteCollection()
        {
            var pageCollection = await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, null);

            await pageCollectionService.DeleteCollectionAsync(pageCollection);

            Assert.Null(await pageCollectionService.FindCollectiondByIdAsync(pageCollection.Id));
        }

        #endregion
    }
}
