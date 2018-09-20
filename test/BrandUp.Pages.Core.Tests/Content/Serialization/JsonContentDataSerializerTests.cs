using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace BrandUp.Pages.Content.Serialization
{
    public class JsonContentDataSerializerTests : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private readonly IContentMetadataManager metadataManager;
        private readonly JsonContentDataSerializer serializer;

        public JsonContentDataSerializerTests()
        {
            var services = new ServiceCollection();

            services.AddWebSiteCore()
                .UseContentTypesFromAssemblies(typeof(TestPageContent).Assembly);

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            metadataManager = serviceScope.ServiceProvider.GetService<IContentMetadataManager>();
            serializer = new JsonContentDataSerializer();
        }

        public void Dispose()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();
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