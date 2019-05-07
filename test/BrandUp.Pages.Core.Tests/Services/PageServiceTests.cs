﻿using BrandUp.Pages.Builder;
using BrandUp.Pages.Content;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.Services
{
    public class PageServiceTests : IAsyncLifetime
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

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            var contentMetadataManager = serviceScope.ServiceProvider.GetService<IContentMetadataManager>();
            pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
            var pageCollectionRepository = serviceScope.ServiceProvider.GetService<IPageCollectionRepositiry>();
            var pageRepository = serviceScope.ServiceProvider.GetService<IPageRepositiry>();

            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var pageCollection = await pageCollectionRepository.CreateCollectionAsync("Test collection", pageType.Name, PageSortMode.FirstOld, null);

            var mainPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, "test", pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(mainPage.Id, "index");

            var testPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, "test", pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(testPage.Id, "test");
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
        public async Task FindPageByPath_EmptyPath()
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
        public async Task SetPageContent()
        {
            var page = await pageService.FindPageByPathAsync(string.Empty);

            var newContent = new TestPageContent { Title = "custom" };
            await pageService.SetPageContentAsync(page, newContent);
            var pageModel = (TestPageContent)await pageService.GetPageContentAsync(page);

            Assert.Equal(newContent.Title, pageModel.Title);
        }
        [Fact]
        public async Task CreatePage()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name, "test");
            Assert.NotNull(page);
            Assert.Equal(pageCollection.Id, page.OwnCollectionId);
            Assert.Equal(pageCollection.PageTypeName, page.TypeName);
            Assert.Equal("test", page.Title);
            Assert.Null(page.UrlPath);
        }
        [Fact]
        public async Task IsPublished_True()
        {
            var page = await pageService.FindPageByPathAsync(string.Empty);

            var result = await pageService.IsPublishedAsync(page);

            Assert.True(result);
        }
        [Fact]
        public async Task IsPublished_False()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name);

            var result = await pageService.IsPublishedAsync(page);

            Assert.False(result);
        }
        [Fact]
        public async Task PublishPage()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name);

            var publishResult = await pageService.PublishPageAsync(page, "test2");

            Assert.Equal("test2", page.UrlPath);
        }

        #endregion
    }
}