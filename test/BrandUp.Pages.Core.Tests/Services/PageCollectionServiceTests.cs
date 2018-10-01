using BrandUp.Pages.Builder;
using BrandUp.Pages.Content;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.Services
{
    public class PageCollectionServiceTests : IDisposable, IAsyncLifetime
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
                .UseFakeViews()
                .AddFakeRepositories();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageService = serviceScope.ServiceProvider.GetService<IPageService>();
            pageCollectionService = serviceScope.ServiceProvider.GetService<IPageCollectionService>();
            pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
        }

        void IDisposable.Dispose()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();
        }

        async Task IAsyncLifetime.InitializeAsync()
        {
            var contentMetadataManager = serviceScope.ServiceProvider.GetService<IContentMetadataManager>();
            var pageCollectionRepository = serviceScope.ServiceProvider.GetService<IPageCollectionRepositiry>();
            var pageRepository = serviceScope.ServiceProvider.GetService<IPageRepositiry>();

            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var pageCollection = await pageCollectionRepository.CreateCollectionAsync("Test collection", pageType.Name, PageSortMode.FirstOld, null);

            var mainPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(mainPage.Id, "main");
            await pageRepository.SetDefaultPageAsync(mainPage);

            var testPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(testPage.Id, "test");
        }
        Task IAsyncLifetime.DisposeAsync()
        {
            return Task.CompletedTask;
        }

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

            var updatedPageCollection = await pageCollectionService.UpdateCollectionAsync(pageCollection.Id, "New title", PageSortMode.FirstNew);

            Assert.NotNull(updatedPageCollection);
            Assert.Equal(pageCollection.Id, updatedPageCollection.Id);
            Assert.Equal("New title", updatedPageCollection.Title);
            Assert.Equal(PageSortMode.FirstNew, updatedPageCollection.SortMode);
        }

        [Fact]
        public async Task DeleteCollection()
        {
            var pageCollection = await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, null);

            await pageCollectionService.DeleteCollectionAsync(pageCollection);

            Assert.Null(await pageCollectionService.FindCollectiondByIdAsync(pageCollection.Id));
        }
    }
}
