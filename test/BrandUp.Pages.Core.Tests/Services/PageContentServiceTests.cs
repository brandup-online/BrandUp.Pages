using BrandUp.Pages.Builder;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Helpers;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Repositories;
using BrandUp.Website;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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

            services.AddPagesCore()
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

        async Task IAsyncLifetime.InitializeAsync()
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
        Task IAsyncLifetime.DisposeAsync()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();

            return Task.CompletedTask;
        }

        #endregion

        #region Test methods

        [Fact]
        public async Task BeginEdit()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");

            var edit = await pageContentService.BeginEditAsync(page);

            Assert.NotNull(edit);
            Assert.Equal(page.Id, edit.PageId);
        }

        [Fact]
        public async Task FindEditById()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await pageContentService.BeginEditAsync(page);

            var result = await pageContentService.FindEditByIdAsync(edit.Id);

            Assert.NotNull(result);
            Assert.Equal(edit.Id, result.Id);
        }

        [Fact]
        public async Task FindEditById_ReturnNull()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");

            var result = await pageContentService.FindEditByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task FindEditByUser()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await pageContentService.BeginEditAsync(page);

            var result = await pageContentService.FindEditByUserAsync(page);

            Assert.NotNull(result);
            Assert.Equal(edit.Id, result.Id);
        }

        [Fact]
        public async Task FindEditByUser_ReturnNull()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");

            var result = await pageContentService.FindEditByUserAsync(page);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetContent()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await pageContentService.BeginEditAsync(page);

            var contentData = (TestPageContent)await pageContentService.GetContentAsync(edit);

            Assert.NotNull(contentData);
            Assert.Equal("test", contentData.Title);
        }

        [Fact]
        public async Task SetContent()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await pageContentService.BeginEditAsync(page);

            var newContent = new TestPageContent() { Title = "test2" };
            await pageContentService.SetContentAsync(edit, newContent);

            var contentData = (TestPageContent)await pageContentService.GetContentAsync(edit);
            Assert.NotNull(contentData);
            Assert.Equal("test2", contentData.Title);
        }

        [Fact]
        public async Task CommitEdit()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await pageContentService.BeginEditAsync(page);

            var newContent = new TestPageContent() { Title = "test2" };
            await pageContentService.SetContentAsync(edit, newContent);
            await pageContentService.CommitEditAsync(edit);

            Assert.Null(await pageContentService.FindEditByIdAsync(edit.Id));

            var contentData = (TestPageContent)await pageService.GetPageContentAsync(page);
            Assert.NotNull(contentData);
            Assert.Equal("test2", contentData.Title);
        }

        [Fact]
        public async Task DiscardEdit()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await pageContentService.BeginEditAsync(page);

            var newContent = new TestPageContent() { Title = "test2" };
            await pageContentService.SetContentAsync(edit, newContent);
            await pageContentService.DiscardEditAsync(edit);

            Assert.Null(await pageContentService.FindEditByIdAsync(edit.Id));

            var contentData = (TestPageContent)await pageService.GetPageContentAsync(page);
            Assert.NotNull(contentData);
            Assert.Equal("test", contentData.Title);
        }

        #endregion
    }
}