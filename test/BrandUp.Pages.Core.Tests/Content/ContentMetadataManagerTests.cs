using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataManagerTests : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private readonly IContentMetadataManager manager;

        public ContentMetadataManagerTests()
        {
            var services = new ServiceCollection();

            services.AddWebSiteCore()
                .UseContentTypesFromAssemblies(typeof(TestPageContent).Assembly);

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            manager = serviceScope.ServiceProvider.GetService<IContentMetadataManager>();
        }

        public void Dispose()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();
        }

        [Fact]
        public void IsRegisterdContentType()
        {
            Assert.True(manager.IsRegisterdContentType(typeof(TestPageContent)));
        }

        [Fact]
        public void GetMetadata()
        {
            var contentType = typeof(TestPageContent);
            var contentMetadata = manager.GetMetadata(contentType);

            Assert.NotNull(contentMetadata);
            Assert.Equal(contentMetadata.ModelType, contentType);
            Assert.Equal("TestPage", contentMetadata.Name);
            Assert.Equal(contentMetadata.Title, TestPageContent.ContentTypeTitle);
        }

        [Fact]
        public void TryGetMetadata()
        {
            var contentType = typeof(TestPageContent);
            var contentMetadata = manager.GetMetadata(contentType);

            Assert.True(manager.TryGetMetadata(contentType, out IContentMetadataProvider contentMetadata2));
            Assert.Equal(contentMetadata, contentMetadata2);
        }

        [Fact]
        public void GetAllMetadata()
        {
            var contentType = typeof(TestPageContent);
            var contentMetadata = manager.GetMetadata(contentType);

            var metadatas = manager.GetAllMetadata();

            Assert.NotNull(metadatas);
            Assert.True(metadatas.Count() > 0);
            Assert.Contains(contentMetadata, metadatas);
        }

        [Fact]
        public void GetDerivedMetadataWithHierarhy()
        {
            var contentType = typeof(TestPageContent);
            var contentMetadata = manager.GetMetadata(contentType);

            var contentMetadatas = contentMetadata.GetDerivedMetadataWithHierarhy(true).ToList();
            Assert.True(contentMetadatas.Count > 0);
            Assert.Contains(contentMetadata, contentMetadatas);
        }

        [Fact]
        public void GetContentViewName_HasValue()
        {
            var viewName = manager.GetContentViewName(new TestPageContent { ViewName = "test" });

            Assert.Equal("test", viewName);
        }

        [Fact]
        public void GetContentViewName_NotValue()
        {
            var viewName = manager.GetContentViewName(new TestPageContent { ViewName = null });

            Assert.Null(viewName);
        }

        [Fact]
        public void ConvertContentModelToDictionary()
        {
            var data = manager.ConvertContentModelToDictionary(new TestPageContent { ViewName = "test" });

            Assert.NotNull(data);
            Assert.True(data.Count > 0);
            Assert.True(data.ContainsKey(ContentMetadataManager.ContentTypeNameDataKey));
            Assert.True(data.ContainsKey("viewName"));
        }

        [Fact]
        public void ConvertDictionaryToContentModel()
        {
            var sourceModel = new TestPageContent { ViewName = "test" };
            var data = manager.ConvertContentModelToDictionary(sourceModel);

            var model = manager.ConvertDictionaryToContentModel(data) as TestPageContent;

            Assert.NotNull(model);
            Assert.Equal(model.GetType(), sourceModel.GetType());
            Assert.Equal(model.ViewName, sourceModel.ViewName);
        }
    }
}