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
            var contentTypeResolver = new AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });
            var contentViewResolver = new Views.AttributesContentViewResolver();

            metadataManager = new ContentMetadataManager(contentTypeResolver, contentViewResolver);
        }

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

            var metadatas = metadataManager.GetAllMetadata();

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

        [Fact]
        public void GetContentViewName_HasValue()
        {
            var viewName = metadataManager.GetContentViewName(new TestPageContent { ViewName = "test" });

            Assert.Equal("test", viewName);
        }

        [Fact]
        public void GetContentViewName_NotValue()
        {
            var viewName = metadataManager.GetContentViewName(new TestPageContent { ViewName = null });

            Assert.Null(viewName);
        }

        [Fact]
        public void ConvertContentModelToDictionary()
        {
            var data = metadataManager.ConvertContentModelToDictionary(new TestPageContent { ViewName = "test" });

            Assert.NotNull(data);
            Assert.True(data.Count > 0);
            Assert.True(data.ContainsKey(ContentMetadataManager.ContentTypeNameDataKey));
            Assert.True(data.ContainsKey("viewName"));
        }

        [Fact]
        public void ConvertDictionaryToContentModel()
        {
            var sourceModel = new TestPageContent { ViewName = "test" };
            var data = metadataManager.ConvertContentModelToDictionary(sourceModel);

            var model = metadataManager.ConvertDictionaryToContentModel(data) as TestPageContent;

            Assert.NotNull(model);
            Assert.Equal(model.GetType(), sourceModel.GetType());
            Assert.Equal(model.ViewName, sourceModel.ViewName);
        }
    }
}