using BrandUp.Pages.Builder;
using BrandUp.Pages.Content;
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
        readonly ContentMetadataManager contentMetadataManager;

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
            contentMetadataManager = serviceScope.ServiceProvider.GetRequiredService<ContentMetadataManager>();
        }

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            await Task.CompletedTask;
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();

            await Task.CompletedTask;
        }

        #endregion

        #region Test methods

        [Fact]
        public async Task BeginEdit()
        {
            var websiteId = "test";
            var contentKey = "test";
            await contentService.SetContentAsync(websiteId, contentKey, TestPageContent.CreateWithOnlyTitle("test"));
            var pageContentMetadata = contentMetadataManager.GetMetadata<TestPageContent>();

            var edit = await contentEditService.BeginEditAsync(websiteId, contentKey, pageContentMetadata);

            Assert.NotNull(edit);
            Assert.Equal(websiteId, edit.WebsiteId);
            Assert.Equal(contentKey, edit.ContentKey);
        }

        [Fact]
        public async Task FindEditById()
        {
            var websiteId = "test";
            var contentKey = "test";
            await contentService.SetContentAsync(websiteId, contentKey, TestPageContent.CreateWithOnlyTitle("test"));
            var pageContentMetadata = contentMetadataManager.GetMetadata<TestPageContent>();
            var edit = await contentEditService.BeginEditAsync(websiteId, contentKey, pageContentMetadata);

            var result = await contentEditService.FindEditByIdAsync(edit.Id);

            Assert.NotNull(result);
            Assert.Equal(edit.Id, result.Id);
        }

        [Fact]
        public async Task FindEditById_ReturnNull()
        {
            var result = await contentEditService.FindEditByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task FindEditByUser()
        {
            var websiteId = "test";
            var contentKey = "test";
            await contentService.SetContentAsync(websiteId, contentKey, TestPageContent.CreateWithOnlyTitle("test"));
            var pageContentMetadata = contentMetadataManager.GetMetadata<TestPageContent>();
            var edit = await contentEditService.BeginEditAsync(websiteId, contentKey, pageContentMetadata);

            var result = await contentEditService.FindEditByUserAsync(websiteId, contentKey);

            Assert.NotNull(result);
            Assert.Equal(edit.Id, result.Id);
        }

        [Fact]
        public async Task FindEditByUser_ReturnNull()
        {
            var websiteId = "test";
            var contentKey = "test";

            var result = await contentEditService.FindEditByUserAsync(websiteId, contentKey);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetContent()
        {
            var websiteId = "test";
            var contentKey = "test";
            await contentService.SetContentAsync(websiteId, contentKey, TestPageContent.CreateWithOnlyTitle("test"));
            var pageContentMetadata = contentMetadataManager.GetMetadata<TestPageContent>();
            var edit = await contentEditService.BeginEditAsync(websiteId, contentKey, pageContentMetadata);

            var contentData = (TestPageContent)await contentEditService.GetContentAsync(edit);

            Assert.NotNull(contentData);
            Assert.Equal("test", contentData.Title);
        }

        [Fact]
        public async Task SetContent()
        {
            var websiteId = "test";
            var contentKey = "test";
            await contentService.SetContentAsync(websiteId, contentKey, TestPageContent.CreateWithOnlyTitle("test"));
            var pageContentMetadata = contentMetadataManager.GetMetadata<TestPageContent>();
            var edit = await contentEditService.BeginEditAsync(websiteId, contentKey, pageContentMetadata);

            var newContent = new TestPageContent() { Title = "test2" };
            await contentEditService.SetContentAsync(edit, newContent);

            var contentData = (TestPageContent)await contentEditService.GetContentAsync(edit);
            Assert.NotNull(contentData);
            Assert.Equal("test2", contentData.Title);
        }

        [Fact]
        public async Task CommitEdit()
        {
            var websiteId = "test";
            var contentKey = "test";
            await contentService.SetContentAsync(websiteId, contentKey, TestPageContent.CreateWithOnlyTitle("test"));
            var pageContentMetadata = contentMetadataManager.GetMetadata<TestPageContent>();
            var edit = await contentEditService.BeginEditAsync(websiteId, contentKey, pageContentMetadata);

            var newContent = new TestPageContent() { Title = "test2" };
            await contentEditService.SetContentAsync(edit, newContent);
            await contentEditService.CommitEditAsync(edit);

            Assert.Null(await contentEditService.FindEditByIdAsync(edit.Id));

            var contentData = (TestPageContent)await contentService.GetContentAsync(edit.WebsiteId, edit.ContentKey);
            Assert.NotNull(contentData);
            Assert.Equal("test2", contentData.Title);
        }

        [Fact]
        public async Task DiscardEdit()
        {
            var websiteId = "test";
            var contentKey = "test";
            await contentService.SetContentAsync(websiteId, contentKey, TestPageContent.CreateWithOnlyTitle("test"));
            var pageContentMetadata = contentMetadataManager.GetMetadata<TestPageContent>();
            var edit = await contentEditService.BeginEditAsync(websiteId, contentKey, pageContentMetadata);

            var newContent = new TestPageContent() { Title = "test2" };
            await contentEditService.SetContentAsync(edit, newContent);
            await contentEditService.DiscardEditAsync(edit);

            Assert.Null(await contentEditService.FindEditByIdAsync(edit.Id));

            var contentData = (TestPageContent)await contentService.GetContentAsync(edit.WebsiteId, edit.ContentKey);
            Assert.NotNull(contentData);
            Assert.Equal("test", contentData.Title);
        }

        #endregion
    }
}