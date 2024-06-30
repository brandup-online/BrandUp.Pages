using BrandUp.Pages.Content.Fakes;
using BrandUp.Pages.Content.Repositories;
using BrandUp.Pages.Repositories;
using BrandUp.Pages.Testing.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Content
{
    public class ContentServiceTest
    {
        readonly ServiceProvider serviceProvider;
        readonly ContentMetadataManager metadataManager;
        readonly IContentRepository contentRepository;
        readonly ContentService contentService;

        public ContentServiceTest()
        {
            var contentTypeResolver = new Infrastructure.AssemblyContentTypeLocator([typeof(ContentModels.TestPageContent).Assembly]);

            var services = new ServiceCollection();
            services.AddSingleton<ContentMetadataManager>();
            services.AddSingleton<Infrastructure.IContentTypeLocator>(contentTypeResolver);
            services.AddSingleton<Infrastructure.IDefaultContentDataProvider, FakeDefaultContentDataProvider>();
            services.AddSingleton<IContentEditRepository, FakeContentEditRepository>();
            services.AddSingleton<IContentRepository, FakeContentRepository>();
            services.AddSingleton<ContentService>();

            serviceProvider = services.BuildServiceProvider();

            metadataManager = serviceProvider.GetRequiredService<ContentMetadataManager>();
            contentRepository = serviceProvider.GetRequiredService<IContentRepository>();
            contentService = serviceProvider.GetRequiredService<ContentService>();
        }

        #region FindContentByKey

        [Fact]
        public async Task FindContentByKey_Success()
        {
            //var contentModel = new ContentModels.TestPageContent { Title = "title" };
            //var contentProvider = metadataManager.GetMetadata(contentModel);
            //var contentTitle = contentProvider.GetContentTitle(contentModel);
            //var contentData = contentProvider.ConvertContentModelToDictionary(contentModel);

            var websiteId = "website";
            var contentKey = "key";
            await contentRepository.CreateContentAsync(websiteId, contentKey);

            var content = await contentService.FindContentAsync(websiteId, contentKey);
            Assert.NotNull(content);
            Assert.NotEqual(Guid.Empty, content.Id);
            Assert.Equal(websiteId, content.WebsiteId);
            Assert.Equal(contentKey, content.Key);
            Assert.Null(content.CommitId);
        }

        #endregion
    }
}