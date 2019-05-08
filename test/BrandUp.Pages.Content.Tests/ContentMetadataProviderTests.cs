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
            var contentTypeResolver = new Infrastructure.AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });

            metadataManager = new ContentMetadataManager(contentTypeResolver);
            contentMetadata = metadataManager.GetMetadata(typeof(TestPageContent));
        }

        #region Test methods

        [Fact]
        public void Properties()
        {
            Assert.Equal(metadataManager, contentMetadata.Manager);
            Assert.Equal(typeof(TestPageContent), contentMetadata.ModelType);
            Assert.Equal("TestPage", contentMetadata.Name);
            Assert.Equal(TestPageContent.ContentTypeTitle, contentMetadata.Title);
            Assert.Equal(TestPageContent.ContentTypeDescription, contentMetadata.Description);
            Assert.Null(contentMetadata.BaseMetadata);
            Assert.Empty(contentMetadata.DerivedContents);
            Assert.NotEmpty(contentMetadata.Fields);
        }

        [Fact]
        public void TryGetField_name_is_original()
        {
            var fieldName = "Title";
            var result = contentMetadata.TryGetField(fieldName, out Fields.FieldProvider field);

            Assert.True(result);
            Assert.NotNull(field);
            Assert.Equal(fieldName, field.Name);
            Assert.Equal("Название", field.Title);
        }

        [Fact]
        public void TryGetField_name_is_lowercase()
        {
            var result = contentMetadata.TryGetField("title", out Fields.FieldProvider field);

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

        [Fact]
        public void ConvertContentModelToDictionary()
        {
            var data = contentMetadata.ConvertContentModelToDictionary(new TestPageContent { Title = "test" });

            Assert.NotNull(data);
            Assert.True(data.Count > 0);
            Assert.True(data.ContainsKey(ContentMetadataProvider.ContentTypeNameDataKey));
            Assert.True(data.ContainsKey("title"));
        }

        [Fact]
        public void ConvertDictionaryToContentModel()
        {
            var sourceModel = new TestPageContent { Title = "test" };
            var data = contentMetadata.ConvertContentModelToDictionary(sourceModel);

            var model = contentMetadata.ConvertDictionaryToContentModel(data) as TestPageContent;

            Assert.NotNull(model);
            Assert.Equal(model.GetType(), sourceModel.GetType());
            Assert.Equal(model.Title, sourceModel.Title);
        }

        #endregion
    }
}