﻿using BrandUp.Pages.ContentModels;
using System.Linq;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataProviderTests
    {
        private readonly IContentMetadataManager metadataManager;

        public ContentMetadataProviderTests()
        {
            var contentTypeResolver = new Infrastructure.AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });

            metadataManager = new ContentMetadataManager(contentTypeResolver);
        }

        #region Test methods

        [Fact]
        public void Properties()
        {
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();

            Assert.Equal(metadataManager, contentMetadata.Manager);
            Assert.Equal(typeof(TestPageContent), contentMetadata.ModelType);
            Assert.Equal("TestPage", contentMetadata.Name);
            Assert.Equal(contentMetadata.Name, contentMetadata.ToString());
            Assert.Equal(TestPageContent.ContentTypeTitle, contentMetadata.Title);
            Assert.Equal(TestPageContent.ContentTypeDescription, contentMetadata.Description);
            Assert.Null(contentMetadata.BaseMetadata);
            Assert.NotEmpty(contentMetadata.DerivedContents);
            Assert.NotEmpty(contentMetadata.Fields);
        }

        [Fact]
        public void TryGetField_name_is_original()
        {
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();
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
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();
            var result = contentMetadata.TryGetField("title", out Fields.FieldProvider field);

            Assert.True(result);
            Assert.NotNull(field);
        }

        [Fact]
        public void CreateModelInstance()
        {
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();
            var model = contentMetadata.CreateModelInstance();

            Assert.NotNull(model);
            Assert.IsType(contentMetadata.ModelType, model);
        }

        [Fact]
        public void ConvertContentModelToDictionary()
        {
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();
            var data = contentMetadata.ConvertContentModelToDictionary(new TestPageContent { Title = "test" });

            Assert.NotNull(data);
            Assert.True(data.Count > 0);
            Assert.True(data.ContainsKey(ContentMetadataProvider.ContentTypeNameDataKey));
            Assert.True(data.ContainsKey("title"));
        }

        [Fact]
        public void ConvertDictionaryToContentModel()
        {
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();
            var sourceModel = new TestPageContent { Title = "test" };
            var data = contentMetadata.ConvertContentModelToDictionary(sourceModel);

            var model = contentMetadata.ConvertDictionaryToContentModel(data) as TestPageContent;

            Assert.NotNull(model);
            Assert.Equal(model.GetType(), sourceModel.GetType());
            Assert.Equal(model.Title, sourceModel.Title);
        }

        [Fact]
        public void GetDerivedMetadataWithHierarhy()
        {
            var contentMetadata = metadataManager.GetMetadata<ArticlePage>();
            var derivedMetadata = contentMetadata.GetDerivedMetadataWithHierarhy(false);

            Assert.Single(derivedMetadata);
            Assert.Equal(derivedMetadata.First(), metadataManager.GetMetadata<NewsPage>());
        }

        [Fact]
        public void GetDerivedMetadataWithHierarhy_IncludeCurrent()
        {
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();
            var derivedMetadata = contentMetadata.GetDerivedMetadataWithHierarhy(true).ToList();

            Assert.Equal(3, derivedMetadata.Count);
            Assert.Equal(derivedMetadata[0], metadataManager.GetMetadata<TestPageContent>());
            Assert.Equal(derivedMetadata[1], metadataManager.GetMetadata<ArticlePage>());
            Assert.Equal(derivedMetadata[2], metadataManager.GetMetadata<NewsPage>());
        }

        #endregion
    }
}