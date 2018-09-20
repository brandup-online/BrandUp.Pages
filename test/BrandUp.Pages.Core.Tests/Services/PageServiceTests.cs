using BrandUp.Pages.Content;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Testing;
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

        public PageServiceTests()
        {
            var services = new ServiceCollection();

            services.AddWebSiteCore()
                .UseContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .UseContentViewsFromAttributes()
                .UseFakeRepositories();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageService = serviceScope.ServiceProvider.GetService<IPageService>();
            pageCollectionService = serviceScope.ServiceProvider.GetService<IPageCollectionService>();
        }

        public void Dispose()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();
        }

        #region IAsyncLifetime members

        public async Task InitializeAsync()
        {
            var contentMetadataManager = serviceScope.ServiceProvider.GetService<IContentMetadataManager>();
            var pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
            var pageCollectionRepository = serviceScope.ServiceProvider.GetService<IPageCollectionRepositiry>();
            var pageRepository = serviceScope.ServiceProvider.GetService<IPageRepositiry>();

            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var pageCollection = await pageCollectionRepository.CreateCollectionAsync("Test collection", pageType.Name, PageSortMode.FirstOld, null);

            var mainPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, contentMetadataManager.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(mainPage.Id, "main");
            await pageRepository.SetDefaultPageAsync(mainPage);

            var testPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, contentMetadataManager.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(testPage.Id, "test");
        }
        public Task DisposeAsync()
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
        public async Task CreatePageAsync()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();

            var page = await pageService.CreatePageAsync(pageCollection);
            Assert.NotNull(page);
            Assert.Equal(page.OwnCollectionId, pageCollection.Id);
            Assert.Equal(page.TypeName, pageCollection.PageTypeName);
        }
    }
}
