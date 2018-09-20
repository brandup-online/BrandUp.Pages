using BrandUp.Pages.ContentModels;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataProviderTests
    {
        private readonly IContentMetadataManager metadataManager;
        private readonly ContentMetadataProvider contentMetadata;

        public ContentMetadataProviderTests()
        {
            metadataManager = new ContentMetadataManager(new AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly }));
            contentMetadata = metadataManager.GetMetadata(typeof(PageHeaderContent));
        }

        [Fact]
        public void Properties()
        {
            Assert.Equal(metadataManager, contentMetadata.Manager);
            Assert.Equal(typeof(PageHeaderContent), contentMetadata.ModelType);
            Assert.Equal("PageHeader", contentMetadata.Name);
            Assert.Equal("Заголовок", contentMetadata.Title);
            Assert.Equal("Заголовок страницы", contentMetadata.Description);
            Assert.Null(contentMetadata.BaseMetadata);
            Assert.Empty(contentMetadata.DerivedContents);
            Assert.NotEmpty(contentMetadata.Fields);
        }

        [Fact]
        public void TryGetField_name_is_original()
        {
            var result = contentMetadata.TryGetField("Title", out Fields.Field field);

            Assert.True(result);
            Assert.NotNull(field);
            Assert.Equal("Название", field.Title);
        }

        [Fact]
        public void TryGetField_name_is_lowercase()
        {
            var result = contentMetadata.TryGetField("title", out Fields.Field field);

            Assert.True(result);
            Assert.NotNull(field);
        }

        [Fact]
        public void CreateModelInstance()
        {
            var model = contentMetadata.CreateModelInstance();

            Assert.NotNull(model);
            Assert.IsType(contentMetadata.ModelType, model);
        }
    }
}