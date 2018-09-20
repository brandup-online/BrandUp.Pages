using BrandUp.Pages.ContentModels;
using System.Collections.Generic;
using Xunit;

namespace BrandUp.Pages.Content.Serialization
{
    public class JsonContentDataSerializerTests
    {
        private readonly IContentMetadataManager metadataManager;
        private readonly JsonContentDataSerializer serializer;

        public JsonContentDataSerializerTests()
        {
            metadataManager = new ContentMetadataManager(new AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly }));
            serializer = new JsonContentDataSerializer();
        }

        [Fact]
        public void SerializeToString()
        {
            var content = new TestPageContent
            {
                Title = "page",
                Header = new PageHeaderContent { Title = "header" },
                Headers = new List<PageHeaderContent> { new PageHeaderContent { Title = "header" } }
            };
            var contentData = metadataManager.ConvertContentModelToDictionary(content);

            var json = serializer.SerializeToString(contentData);
            Assert.NotNull(json);
        }

        [Fact]
        public void DeserializeFromString()
        {
            var content = new TestPageContent
            {
                Title = "page",
                Header = new PageHeaderContent { Title = "header" },
                Headers = new List<PageHeaderContent> { new PageHeaderContent { Title = "header" } }
            };
            var contentData = metadataManager.ConvertContentModelToDictionary(content);
            var json = serializer.SerializeToString(contentData);

            var deserializedContentData = serializer.DeserializeFromString(json);
            var deserializedContent = (TestPageContent)metadataManager.ConvertDictionaryToContentModel(deserializedContentData);

            Assert.NotNull(deserializedContent);
            Assert.Equal(deserializedContent.Title, content.Title);
            Assert.Equal(deserializedContent.Header.Title, content.Header.Title);
            Assert.Equal(deserializedContent.Headers[0].Title, content.Headers[0].Title);
        }
    }
}