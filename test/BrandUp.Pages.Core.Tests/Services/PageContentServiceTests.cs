using BrandUp.Pages.Builder;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Helpers;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Website;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Services
{
    public class PageContentServiceTests : IAsyncLifetime
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private readonly IPageService pageService;
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageContentService pageContentService;
        private IPageMetadataManager pageMetadataManager;
        readonly IWebsiteContext websiteContext;

        public PageContentServiceTests()
        {
            websiteContext = new TestWebsiteContext("test", "test");

            var services = new ServiceCollection();

            services.AddPages()
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakes();

            services.AddSingleton(websiteContext);

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageService = serviceScope.ServiceProvider.GetService<IPageService>();
            pageCollectionService = serviceScope.ServiceProvider.GetService<IPageCollectionService>();
            pageContentService = serviceScope.ServiceProvider.GetService<IPageContentService>();
            pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
        }

        #region IAsyncLifetime members

        async ValueTask IAsyncLifetime.InitializeAsync()
        {
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
        public async Task BeginEdit()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);

            var edit = await pageContentService.BeginEditAsync(page, TestContext.Current.CancellationToken);

            Assert.NotNull(edit);
            Assert.Equal(page.Id, edit.PageId);
        }

        [Fact]
        public async Task FindEditById()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            var edit = await pageContentService.BeginEditAsync(page, TestContext.Current.CancellationToken);

            var result = await pageContentService.FindEditByIdAsync(edit.Id, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(edit.Id, result.Id);
        }

        [Fact]
        public async Task FindEditById_ReturnNull()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);

            var result = await pageContentService.FindEditByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            Assert.Null(result);
        }

        [Fact]
        public async Task FindEditByUser()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            var edit = await pageContentService.BeginEditAsync(page, TestContext.Current.CancellationToken);

            var result = await pageContentService.FindEditByUserAsync(page, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(edit.Id, result.Id);
        }

        [Fact]
        public async Task FindEditByUser_ReturnNull()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);

            var result = await pageContentService.FindEditByUserAsync(page, TestContext.Current.CancellationToken);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetContent()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            var edit = await pageContentService.BeginEditAsync(page, TestContext.Current.CancellationToken);

            var contentData = (TestPageContent)await pageContentService.GetContentAsync(edit, TestContext.Current.CancellationToken);

            Assert.NotNull(contentData);
            Assert.Equal("test", contentData.Title);
        }

        [Fact]
        public async Task SetContent()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            var edit = await pageContentService.BeginEditAsync(page, TestContext.Current.CancellationToken);

            var newContent = new TestPageContent() { Title = "test2" };
            await pageContentService.SetContentAsync(edit, newContent, TestContext.Current.CancellationToken);

            var contentData = (TestPageContent)await pageContentService.GetContentAsync(edit, TestContext.Current.CancellationToken);
            Assert.NotNull(contentData);
            Assert.Equal("test2", contentData.Title);
        }

        [Fact]
        public async Task CommitEdit()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            var edit = await pageContentService.BeginEditAsync(page, TestContext.Current.CancellationToken);

            var newContent = new TestPageContent() { Title = "test2" };
            await pageContentService.SetContentAsync(edit, newContent, TestContext.Current.CancellationToken);
            await pageContentService.CommitEditAsync(edit, TestContext.Current.CancellationToken);

            Assert.Null(await pageContentService.FindEditByIdAsync(edit.Id, TestContext.Current.CancellationToken));

            var contentData = (TestPageContent)await pageService.GetPageContentAsync(page, TestContext.Current.CancellationToken);
            Assert.NotNull(contentData);
            Assert.Equal("test2", contentData.Title);
        }

        [Fact]
        public async Task DiscardEdit()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test", TestContext.Current.CancellationToken);
            var edit = await pageContentService.BeginEditAsync(page, TestContext.Current.CancellationToken);

            var newContent = new TestPageContent() { Title = "test2" };
            await pageContentService.SetContentAsync(edit, newContent, TestContext.Current.CancellationToken);
            await pageContentService.DiscardEditAsync(edit, TestContext.Current.CancellationToken);

            Assert.Null(await pageContentService.FindEditByIdAsync(edit.Id, TestContext.Current.CancellationToken));

            var contentData = (TestPageContent)await pageService.GetPageContentAsync(page, TestContext.Current.CancellationToken);
            Assert.NotNull(contentData);
            Assert.Equal("test", contentData.Title);
        }

        #endregion
    }
}