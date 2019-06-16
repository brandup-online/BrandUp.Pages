using BrandUp.Pages.Builder;
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
    public class PageServiceTests : IAsyncLifetime
    {
        const string DefaultPageHeader = "New page";
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private readonly IPageService pageService;
        private readonly IPageCollectionService pageCollectionService;
        private IPageMetadataManager pageMetadataManager;

        public PageServiceTests()
        {
            var services = new ServiceCollection();

            services.AddPages(options =>
            {
                options.DefaultPageHeader = DefaultPageHeader;
            })
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakes();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageService = serviceScope.ServiceProvider.GetService<IPageService>();
            pageCollectionService = serviceScope.ServiceProvider.GetService<IPageCollectionService>();
        }

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
            var pageCollectionRepository = serviceScope.ServiceProvider.GetService<IPageCollectionRepository>();
            var pageRepository = serviceScope.ServiceProvider.GetService<IPageRepository>();

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
        public async Task CreatePage_WithContentModel()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();

            var page = await pageService.CreatePageAsync(pageCollection, new TestPageContent { Title = "title" });

            Assert.NotNull(page);
            Assert.Equal(pageCollection.Id, page.OwnCollectionId);
            Assert.Equal(pageCollection.PageTypeName, page.TypeName);
            Assert.Equal("title", page.Header);
            Assert.NotNull(page.UrlPath);
        }
        [Fact]
        public async Task CreatePage_WithDefaultHeader()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name);
            Assert.NotNull(page);
            Assert.Equal(pageCollection.Id, page.OwnCollectionId);
            Assert.Equal(pageCollection.PageTypeName, page.TypeName);
            Assert.Equal(TestPageContent.ContentTypeTitle, page.Header);
            Assert.NotNull(page.UrlPath);
        }
        [Fact]
        public async Task CreatePage_WithSpecifyHeader()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name, "test");
            Assert.NotNull(page);
            Assert.Equal(pageCollection.Id, page.OwnCollectionId);
            Assert.Equal(pageCollection.PageTypeName, page.TypeName);
            Assert.Equal("test", page.Header);
            Assert.NotNull(page.UrlPath);
        }
        [Fact]
        public async Task CreatePage_Fail_PageTypeNotAllowered()
        {
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(ArticlePageContent));
            var pageCollection = (await pageCollectionService.CreateCollectionAsync("test", pageType.Name, PageSortMode.FirstOld, null)).Data;

            pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            try
            {
                var page = await pageService.CreatePageAsync(pageCollection, pageType.Name, "test");
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
        }
        [Fact]
        public async Task IsPublished_True()
        {
            var page = await pageService.FindPageByPathAsync(string.Empty);

            var result = page.IsPublished;

            Assert.True(result);
        }
        [Fact]
        public async Task IsPublished_False()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name);

            var result = page.IsPublished;

            Assert.False(result);
        }
        [Fact]
        public async Task PublishPage()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name);

            var publishResult = await pageService.PublishPageAsync(page, "test2");

            Assert.True(publishResult.Succeeded);
            Assert.Equal("test2", page.UrlPath);
        }
        [Fact]
        public async Task PublishPage_Fail_PageUrlExist()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name);

            var publishResult = await pageService.PublishPageAsync(page, "test");

            Assert.False(publishResult.Succeeded);
        }
        [Fact]
        public async Task PublishPage_Fail_AlreadyPublished()
        {
            var page = await pageService.FindPageByPathAsync("test");

            var publishResult = await pageService.PublishPageAsync(page, "test2");

            Assert.False(publishResult.Succeeded);
        }

        [Fact]
        public async Task GetPageSeoOptions()
        {
            var page = await pageService.FindPageByPathAsync("test");
            var seo = await pageService.GetPageSeoOptionsAsync(page);

            Assert.NotNull(seo);
            Assert.Null(seo.Title);
            Assert.Null(seo.Description);
            Assert.Null(seo.Keywords);
        }

        [Fact]
        public async Task UpdatePageSeoOptions_Title()
        {
            var page = await pageService.FindPageByPathAsync("test");
            await pageService.UpdatePageSeoOptionsAsync(page, new PageSeoOptions { Title = "test" });

            var seo = await pageService.GetPageSeoOptionsAsync(page);

            Assert.NotNull(seo);
            Assert.Equal("test", seo.Title);
            Assert.Null(seo.Description);
            Assert.Null(seo.Keywords);
        }

        [Fact]
        public async Task UpdatePageSeoOptions_Description()
        {
            var page = await pageService.FindPageByPathAsync("test");
            await pageService.UpdatePageSeoOptionsAsync(page, new PageSeoOptions { Description = "test" });

            var seo = await pageService.GetPageSeoOptionsAsync(page);

            Assert.NotNull(seo);
            Assert.Null(seo.Title);
            Assert.Equal("test", seo.Description);
            Assert.Null(seo.Keywords);
        }

        [Fact]
        public async Task UpdatePageSeoOptions_Keywords()
        {
            var page = await pageService.FindPageByPathAsync("test");
            await pageService.UpdatePageSeoOptionsAsync(page, new PageSeoOptions { Keywords = new string[] { "test" } });

            var seo = await pageService.GetPageSeoOptionsAsync(page);

            Assert.NotNull(seo);
            Assert.Null(seo.Title);
            Assert.Null(seo.Description);
            Assert.Contains("test", seo.Keywords);
        }

        #endregion
    }
}