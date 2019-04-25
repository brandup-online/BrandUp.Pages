using BrandUp.Pages.Builder;
using BrandUp.Pages.Content;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.Services
{
    public class PageServiceTests : IDisposable, IAsyncLifetime
    {
        private ServiceProvider serviceProvider;
        private IServiceScope serviceScope;
        private readonly IPageService pageService;
        private readonly IPageCollectionService pageCollectionService;
        private IPageMetadataManager pageMetadataManager;

        public PageServiceTests()
        {
            var services = new ServiceCollection();

            services.AddPages()
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakeRepositories();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageService = serviceScope.ServiceProvider.GetService<IPageService>();
            pageCollectionService = serviceScope.ServiceProvider.GetService<IPageCollectionService>();
        }

        void IDisposable.Dispose()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();
        }

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            var contentMetadataManager = serviceScope.ServiceProvider.GetService<IContentMetadataManager>();
            pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
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

        #endregion

        [Fact]
        public async Task FindPageByPath_RootPath()
        {
            var page = await pageService.FindPageByPathAsync(string.Empty);

            Assert.NotNull(page);
        }
        [Fact]
        public async Task FindPageByPath_SpecifyPath()
        {
            var page = await pageService.FindPageByPathAsync("test");

            Assert.NotNull(page);
        }
        [Fact]
        public async Task GetDefaultPage()
        {
            var page = await pageService.GetDefaultPageAsync();

            Assert.NotNull(page);
        }
        [Fact]
        public async Task SetDefaultPage()
        {
            var testPage = await pageService.FindPageByPathAsync("test");
            await pageService.SetDefaultPageAsync(testPage);

            var defaultPage = await pageService.GetDefaultPageAsync();

            Assert.Equal(testPage.Id, defaultPage.Id);
        }
        [Fact]
        public async Task GetPageType()
        {
            var page = await pageService.FindPageByPathAsync(string.Empty);

            var pageType = await pageService.GetPageTypeAsync(page);

            Assert.NotNull(pageType);
        }
        [Fact]
        public async Task GetPageContent()
        {
            var page = await pageService.FindPageByPathAsync(string.Empty);

            var pageModel = await pageService.GetPageContentAsync(page);

            Assert.NotNull(pageModel);
        }
        [Fact]
        public async Task SetPageContent()
        {
            var page = await pageService.FindPageByPathAsync(string.Empty);

            var newContent = new TestPageContent { Title = "custom" };
            await pageService.SetPageContentAsync(page, newContent);
            var pageModel = (TestPageContent)await pageService.GetPageContentAsync(page);

            Assert.Equal(newContent.Title, pageModel.Title);
        }
        [Fact]
        public async Task CreatePageAsync()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name);
            Assert.NotNull(page);
            Assert.Equal(page.OwnCollectionId, pageCollection.Id);
            Assert.Equal(page.TypeName, pageCollection.PageTypeName);
        }
    }
}