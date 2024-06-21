using BrandUp.Pages.Builder;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Helpers;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Website;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Services
{
    public class ContentEditServiceTests : IAsyncLifetime
    {
        readonly ServiceProvider serviceProvider;
        readonly IServiceScope serviceScope;
        readonly IPageService pageService;
        readonly IPageCollectionService pageCollectionService;
        readonly IContentEditService contentEditService;
        readonly ContentService contentService;
        readonly IPageMetadataManager pageMetadataManager;
        readonly IWebsiteContext websiteContext;

        public ContentEditServiceTests()
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
            contentEditService = serviceScope.ServiceProvider.GetService<IContentEditService>();
            contentService = serviceScope.ServiceProvider.GetService<ContentService>();
            pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
        }

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            var pageCollectionRepository = serviceScope.ServiceProvider.GetService<IPageCollectionRepository>();
            var pageRepository = serviceScope.ServiceProvider.GetService<IPageRepository>();

            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var pageCollection = await pageCollectionRepository.CreateCollectionAsync("test", "Test collection", pageType.Name, PageSortMode.FirstOld, null);

            var pageId = Guid.NewGuid();
            var contentKey = await pageService.GetContentKeyAsync(pageId);
            var mainPage = await pageRepository.CreatePageAsync("test", pageCollection.Id, pageId, pageType.Name, "test", contentKey, pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(mainPage, "index");
            await pageRepository.UpdatePageAsync(mainPage);

            pageId = Guid.NewGuid();
            contentKey = await pageService.GetContentKeyAsync(pageId);
            var testPage = await pageRepository.CreatePageAsync("test", pageCollection.Id, pageId, pageType.Name, "test", contentKey, pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
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

            var edit = await contentEditService.BeginEditAsync(page);

            Assert.NotNull(edit);
            Assert.Equal(page.Id, edit.PageId);
        }

        [Fact]
        public async Task FindEditById()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await contentEditService.BeginEditAsync(page);

            var result = await contentEditService.FindEditByIdAsync(edit.Id);

            Assert.NotNull(result);
            Assert.Equal(edit.Id, result.Id);
        }

        [Fact]
        public async Task FindEditById_ReturnNull()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");

            var result = await contentEditService.FindEditByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task FindEditByUser()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await contentEditService.BeginEditAsync(page);

            var result = await contentEditService.FindEditByUserAsync(page);

            Assert.NotNull(result);
            Assert.Equal(edit.Id, result.Id);
        }

        [Fact]
        public async Task FindEditByUser_ReturnNull()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");

            var result = await contentEditService.FindEditByUserAsync(page);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetContent()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await contentEditService.BeginEditAsync(page);

            var contentData = (TestPageContent)await contentEditService.GetContentAsync(edit);

            Assert.NotNull(contentData);
            Assert.Equal("test", contentData.Title);
        }

        [Fact]
        public async Task SetContent()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await contentEditService.BeginEditAsync(page);

            var newContent = new TestPageContent() { Title = "test2" };
            await contentEditService.SetContentAsync(edit, newContent);

            var contentData = (TestPageContent)await contentEditService.GetContentAsync(edit);
            Assert.NotNull(contentData);
            Assert.Equal("test2", contentData.Title);
        }

        [Fact]
        public async Task CommitEdit()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await contentEditService.BeginEditAsync(page);

            var newContent = new TestPageContent() { Title = "test2" };
            await contentEditService.SetContentAsync(edit, newContent);
            await contentEditService.CommitEditAsync(edit);

            Assert.Null(await contentEditService.FindEditByIdAsync(edit.Id));

            var contentData = (TestPageContent)await contentService.GetContentAsync(page.WebsiteId, await pageService.GetContentKeyAsync(page.Id));
            Assert.NotNull(contentData);
            Assert.Equal("test2", contentData.Title);
        }

        [Fact]
        public async Task DiscardEdit()
        {
            var page = await pageService.FindPageByPathAsync(websiteContext.Website.Id, "test");
            var edit = await contentEditService.BeginEditAsync(page);

            var newContent = new TestPageContent() { Title = "test2" };
            await contentEditService.SetContentAsync(edit, newContent);
            await contentEditService.DiscardEditAsync(edit);

            Assert.Null(await contentEditService.FindEditByIdAsync(edit.Id));

            var contentData = (TestPageContent)await contentService.GetContentAsync(page.WebsiteId, await pageService.GetContentKeyAsync(page.Id));
            Assert.NotNull(contentData);
            Assert.Equal("test", contentData.Title);
        }

        #endregion
    }
}