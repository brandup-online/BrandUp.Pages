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
            var contentTypeResolver = new AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });
            var contentViewResolver = new Views.AttributesContentViewResolver();

            metadataManager = new ContentMetadataManager(contentTypeResolver, contentViewResolver);
            contentMetadata = metadataManager.GetMetadata(typeof(TestPageContent));
        }

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
            Assert.True(contentMetadata.SupportViews);
            Assert.True(contentMetadata.HasViews);
            Assert.NotEmpty(contentMetadata.Views);
        }

        [Fact]
        public void TryGetField_name_is_original()
        {
            var fieldName = "Title";
            var result = contentMetadata.TryGetField(fieldName, out Fields.Field field);

            Assert.True(result);
            Assert.NotNull(field);
            Assert.Equal(fieldName, field.Name);
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
        public void TryGetView()
        {
            var viewName = "TestPage.Default";
            var result = contentMetadata.TryGetView(viewName, out Views.ContentView view);

            Assert.True(result);
            Assert.NotNull(view);
            Assert.Equal(viewName, view.Name);
            Assert.Equal(viewName, view.Title);
        }

        [Fact]
        public void GetViewName_ReturnDefault()
        {
            var viewName = contentMetadata.GetViewName(new TestPageContent());

            Assert.Equal(contentMetadata.DefaultView.Name, viewName);
        }

        [Fact]
        public void GetViewName_ReturnSpecify()
        {
            var viewNameValue = "TestPage.Default";
            var viewName = contentMetadata.GetViewName(new TestPageContent() { ViewName = viewNameValue });

            Assert.Equal(viewNameValue, viewName);
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
            var data = contentMetadata.ConvertContentModelToDictionary(new TestPageContent { ViewName = "test" });

            Assert.NotNull(data);
            Assert.True(data.Count > 0);
            Assert.True(data.ContainsKey(ContentMetadataProvider.ContentTypeNameDataKey));
            Assert.True(data.ContainsKey("viewName"));
        }

        [Fact]
        public void ConvertDictionaryToContentModel()
        {
            var sourceModel = new TestPageContent { ViewName = "test" };
            var data = contentMetadata.ConvertContentModelToDictionary(sourceModel);

            var model = contentMetadata.ConvertDictionaryToContentModel(data) as TestPageContent;

            Assert.NotNull(model);
            Assert.Equal(model.GetType(), sourceModel.GetType());
            Assert.Equal(model.ViewName, sourceModel.ViewName);
        }
    }
}