using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Infrastructure;
using BrandUp.Pages.ContentModels;

namespace BrandUp.Pages.Metadata
{
    public class PageMetadataManagerTests
    {
        private readonly PageMetadataManager pageMetadataManager;

        public PageMetadataManagerTests()
        {
            var contentTypeResolver = new AssemblyContentTypeLocator([typeof(TestPageContent).Assembly]);
            var contentMetadataManager = new ContentMetadataManager(contentTypeResolver);

            pageMetadataManager = new PageMetadataManager(contentMetadataManager);
        }

        [Fact]
        public void GetAllMetadata()
        {
            var metadatas = pageMetadataManager.MetadataProviders;

            Assert.NotEmpty(metadatas);
        }

        [Fact]
        public void FindPageMetadataByContentType()
        {
            var pageContentType = typeof(TestPageContent);
            var pageMetadata = pageMetadataManager.FindPageMetadataByContentType(pageContentType);

            Assert.NotNull(pageMetadata);
            Assert.Equal(pageContentType, pageMetadata.ContentType);
        }

        [Fact]
        public void FindPageMetadataByName()
        {
            var pageContentType = typeof(TestPageContent);
            var pageTypeName = "TestPage";
            var pageMetadata = pageMetadataManager.FindPageMetadataByName(pageTypeName);

            Assert.NotNull(pageMetadata);
            Assert.Equal(pageTypeName, pageMetadata.Name);
        }
    }
}