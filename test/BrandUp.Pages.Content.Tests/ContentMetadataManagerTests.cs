using BrandUp.Pages.ContentModels;
using System.Linq;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataManagerTests
    {
        private readonly IContentMetadataManager metadataManager;

        public ContentMetadataManagerTests()
        {
            var contentTypeResolver = new Infrastructure.AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });

            metadataManager = new ContentMetadataManager(contentTypeResolver);
        }

        #region Test methods

        [Fact]
        public void IsRegisterdContentType()
        {
            Assert.True(metadataManager.IsRegisterdContentType(typeof(TestPageContent)));
        }

        [Fact]
        public void GetMetadata()
        {
            var contentType = typeof(TestPageContent);
            var contentMetadata = metadataManager.GetMetadata(contentType);

            Assert.NotNull(contentMetadata);
            Assert.Equal(contentMetadata.ModelType, contentType);
            Assert.Equal("TestPage", contentMetadata.Name);
            Assert.Equal(contentMetadata.Title, TestPageContent.ContentTypeTitle);
        }

        [Fact]
        public void TryGetMetadata()
        {
            var contentType = typeof(TestPageContent);
            var contentMetadata = metadataManager.GetMetadata(contentType);

            Assert.True(metadataManager.TryGetMetadata(contentType, out ContentMetadataProvider contentMetadata2));
            Assert.Equal(contentMetadata, contentMetadata2);
        }

        [Fact]
        public void GetAllMetadata()
        {
            var contentType = typeof(TestPageContent);
            var contentMetadata = metadataManager.GetMetadata(contentType);

            var metadatas = metadataManager.MetadataProviders;

            Assert.NotNull(metadatas);
            Assert.True(metadatas.Count() > 0);
            Assert.Contains(contentMetadata, metadatas);
        }

        [Fact]
        public void GetDerivedMetadataWithHierarhy()
        {
            var contentType = typeof(TestPageContent);
            var contentMetadata = metadataManager.GetMetadata(contentType);

            var contentMetadatas = contentMetadata.GetDerivedMetadataWithHierarhy(true).ToList();
            Assert.True(contentMetadatas.Count > 0);
            Assert.Contains(contentMetadata, contentMetadatas);
        }

        #endregion
    }
}