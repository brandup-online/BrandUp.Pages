using BrandUp.Pages.Builder;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Helpers;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Website;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Services
{
    public class PageServiceTests : IAsyncLifetime
    {
        const string DefaultPageHeader = "New page";
        readonly ServiceProvider serviceProvider;
        readonly IServiceScope serviceScope;
        readonly IPageService pageService;
        readonly IPageCollectionService pageCollectionService;
        private IPageMetadataManager pageMetadataManager;
        readonly IWebsiteContext websiteContext;

        public PageServiceTests()
        {
            websiteContext = new TestWebsiteContext("test", "test");

            var services = new ServiceCollection();

            services.AddPages(options =>
            {
                options.DefaultPageHeader = DefaultPageHeader;
            })
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakes();

            services.AddSingleton(websiteContext);

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageService = serviceScope.ServiceProvider.GetService<IPageService>();
            pageCollectionService = serviceScope.ServiceProvider.GetService<IPageCollectionService>();
        }

        #region IAsyncLifetime members

        async ValueTask IAsyncLifetime.InitializeAsync()
        {
            pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
            var pageCollectionRepository = serviceScope.ServiceProvider.GetService<IPageCollectionRepository>();
            var pageRepository = serviceScope.ServiceProvider.GetService<IPageRepository>();

            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var pageCollection = await pageCollectionRepository.CreateCollectionAsync("test", "Test collection", pageType.Name, PageSortMode.FirstOld, null);

            var mainPage = await pageRepository.CreatePageAsync("test", pageCollection.Id, pageType.Name, "test", pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(mainPage, "index");
            await pageRepository.UpdatePageAsync(mainPage);

            var testPage = await pageRepository.CreatePageAsync("test", pageCollection.Id, pageType.Name, "test", pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(testPage, "test");
            await pageRepository.UpdatePageAsync(testPage);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();
            await serviceProvider.DisposeAsync();
        }

        #endregion

        #region Test methods

        [Fact]
        public async Task FindPageByPath_EmptyPath()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, string.Empty, TestContext.Current.CancellationToken);

            Assert.NotNull(page);
        }
        [Fact]
        public async Task FindPageByPath_SpecifyPath()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);

            Assert.NotNull(page);
        }
        [Fact]
        public async Task GetDefaultPage()
        {
            var page = await pageService.GetDefaultPageAsync(websiteContext.Website.Id, TestContext.Current.CancellationToken);

            Assert.NotNull(page);
        }
        [Fact]
        public async Task GetPageType()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, string.Empty, TestContext.Current.CancellationToken);

            var pageType = await pageService.GetPageTypeAsync(page, TestContext.Current.CancellationToken);

            Assert.NotNull(pageType);
        }
        [Fact]
        public async Task GetPageContent()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, string.Empty, TestContext.Current.CancellationToken);

            var pageModel = await pageService.GetPageContentAsync(page, TestContext.Current.CancellationToken);

            Assert.NotNull(pageModel);
        }
        [Fact]
        public async Task SetPageContent()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, string.Empty, TestContext.Current.CancellationToken);

            var newContent = new TestPageContent { Title = "custom" };
            await pageService.SetPageContentAsync(page, newContent, TestContext.Current.CancellationToken);
            var pageModel = (TestPageContent)await pageService.GetPageContentAsync(page, TestContext.Current.CancellationToken);

            Assert.Equal(newContent.Title, pageModel.Title);
        }
        [Fact]
        public async Task CreatePage_WithContentModel()
        {
            var pageCollection = (await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id)).First();

            var page = await pageService.CreatePageAsync(pageCollection, new TestPageContent { Title = "title" }, TestContext.Current.CancellationToken);

            Assert.NotNull(page);
            Assert.Equal(pageCollection.Id, page.OwnCollectionId);
            Assert.Equal(pageCollection.PageTypeName, page.TypeName);
            Assert.Equal("title", page.Header);
            Assert.NotNull(page.UrlPath);
        }
        [Fact]
        public async Task CreatePage_WithDefaultHeader()
        {
            var pageCollection = (await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name, cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(page);
            Assert.Equal(pageCollection.Id, page.OwnCollectionId);
            Assert.Equal(pageCollection.PageTypeName, page.TypeName);
            Assert.Equal(TestPageContent.ContentTypeTitle, page.Header);
            Assert.NotNull(page.UrlPath);
        }
        [Fact]
        public async Task CreatePage_WithSpecifyHeader()
        {
            var pageCollection = (await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name, "test", TestContext.Current.CancellationToken);
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
            var pageCollection = (await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageType.Name, PageSortMode.FirstOld)).Data;

            pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            try
            {
                var page = await pageService.CreatePageAsync(pageCollection, pageType.Name, "test", TestContext.Current.CancellationToken);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
        }
        [Fact]
        public async Task IsPublished_True()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, string.Empty, TestContext.Current.CancellationToken);

            var result = page.IsPublished;

            Assert.True(result);
        }
        [Fact]
        public async Task IsPublished_False()
        {
            var pageCollection = (await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name, cancellationToken: TestContext.Current.CancellationToken);

            var result = page.IsPublished;

            Assert.False(result);
        }
        [Fact]
        public async Task PublishPage()
        {
            var pageCollection = (await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name, cancellationToken: TestContext.Current.CancellationToken);

            var publishResult = await pageService.PublishPageAsync(page, "test2", TestContext.Current.CancellationToken);

            Assert.True(publishResult.IsSuccess);
            Assert.Equal("test2", page.UrlPath);
        }
        [Fact]
        public async Task PublishPage_Fail_PageUrlExist()
        {
            var pageCollection = (await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id)).First();
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var page = await pageService.CreatePageAsync(pageCollection, pageType.Name, cancellationToken: TestContext.Current.CancellationToken);

            var publishResult = await pageService.PublishPageAsync(page, "test", TestContext.Current.CancellationToken);

            Assert.False(publishResult.IsSuccess);
        }
        [Fact]
        public async Task PublishPage_Fail_AlreadyPublished()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);

            var publishResult = await pageService.PublishPageAsync(page, "test2", TestContext.Current.CancellationToken);

            Assert.False(publishResult.IsSuccess);
        }

        [Fact]
        public async Task GetPageSeoOptions()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            var seo = await pageService.GetPageSeoOptionsAsync(page, TestContext.Current.CancellationToken);

            Assert.NotNull(seo);
            Assert.Null(seo.Title);
            Assert.Null(seo.Description);
            Assert.Null(seo.Keywords);
        }

        [Fact]
        public async Task UpdatePageSeoOptions_Title()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            await pageService.UpdatePageSeoOptionsAsync(page, new PageSeoOptions { Title = "test" }, TestContext.Current.CancellationToken);

            var seo = await pageService.GetPageSeoOptionsAsync(page, TestContext.Current.CancellationToken);

            Assert.NotNull(seo);
            Assert.Equal("test", seo.Title);
            Assert.Null(seo.Description);
            Assert.Null(seo.Keywords);
        }

        [Fact]
        public async Task UpdatePageSeoOptions_Description()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            await pageService.UpdatePageSeoOptionsAsync(page, new PageSeoOptions { Description = "test" }, TestContext.Current.CancellationToken);

            var seo = await pageService.GetPageSeoOptionsAsync(page, TestContext.Current.CancellationToken);

            Assert.NotNull(seo);
            Assert.Null(seo.Title);
            Assert.Equal("test", seo.Description);
            Assert.Null(seo.Keywords);
        }

        [Fact]
        public async Task UpdatePageSeoOptions_Keywords()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            await pageService.UpdatePageSeoOptionsAsync(page, new PageSeoOptions { Keywords = ["test"] }, TestContext.Current.CancellationToken);

            var seo = await pageService.GetPageSeoOptionsAsync(page, TestContext.Current.CancellationToken);

            Assert.NotNull(seo);
            Assert.Null(seo.Title);
            Assert.Null(seo.Description);
            Assert.Contains("test", seo.Keywords);
        }

        #endregion
    }
}