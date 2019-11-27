using BrandUp.Pages.ContentModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataProviderTests
    {
        private readonly IContentMetadataManager metadataManager;

        public ContentMetadataProviderTests()
        {
            var contentTypeResolver = new Infrastructure.AssemblyContentTypeLocator(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });

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
            var result = contentMetadata.TryGetField(fieldName, out Fields.FieldProviderAttribute field);

            Assert.True(result);
            Assert.NotNull(field);
            Assert.Equal(fieldName, field.Name);
            Assert.Equal("Название", field.Title);
        }

        [Fact]
        public void TryGetField_name_is_lowercase()
        {
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();
            var result = contentMetadata.TryGetField("title", out Fields.FieldProviderAttribute field);

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

        [Fact]
        public void IsInherited()
        {
            var contentMetadata1 = metadataManager.GetMetadata<ArticlePage>();
            var contentMetadata2 = metadataManager.GetMetadata<NewsPage>();

            Assert.False(contentMetadata1.IsInherited(contentMetadata1));
            Assert.False(contentMetadata1.IsInherited(contentMetadata2));
            Assert.True(contentMetadata2.IsInherited(contentMetadata1));
        }

        [Fact]
        public void IsInheritedOrEqual()
        {
            var contentMetadata1 = metadataManager.GetMetadata<ArticlePage>();
            var contentMetadata2 = metadataManager.GetMetadata<NewsPage>();

            Assert.True(contentMetadata1.IsInheritedOrEqual(contentMetadata1));
            Assert.False(contentMetadata1.IsInheritedOrEqual(contentMetadata2));
            Assert.True(contentMetadata2.IsInheritedOrEqual(contentMetadata1));
        }

        [Fact]
        public void GetContentTitle()
        {
            var contentMetadata = metadataManager.GetMetadata<ArticlePage>();

            var content = new ArticlePage { Title = "test" };
            var contentTitle = contentMetadata.GetContentTitle(content);

            Assert.Equal(content.Title, contentTitle);
        }

        [Fact]
        public void GetContentTitle_NoAttr()
        {
            var contentMetadata = metadataManager.GetMetadata<PageHeaderContent>();

            var content = new PageHeaderContent();
            var contentTitle = contentMetadata.GetContentTitle(content);

            Assert.Equal(contentMetadata.Title, contentTitle);
        }

        [Fact]
        public void SetContentTitle()
        {
            var contentMetadata = metadataManager.GetMetadata<ArticlePage>();

            var content = new ArticlePage { Title = "test" };
            contentMetadata.SetContentTitle(content, "test2");

            Assert.Equal("test2", content.Title);
        }

        [Fact]
        public void Implicit_Type()
        {
            var contentMetadata = metadataManager.GetMetadata<ArticlePage>();
            var type = (Type)contentMetadata;

            Assert.True(contentMetadata == typeof(ArticlePage));
            Assert.Equal(typeof(ArticlePage), type);
        }

        [Fact]
        public void Implicit_Null_Type()
        {
            ContentMetadataProvider contentMetadata = null;
            var type = (Type)contentMetadata;

            Assert.Null(type);
        }

        [Fact]
        public void ApplyInjections()
        {
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();
            var services = new ServiceCollection().AddScoped<TestService>();
            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    var page = TestPageContent.Create("test", new PageHeaderContent(), new List<PageHeaderContent> { new PageHeaderContent() });

                    contentMetadata.ApplyInjections(page, serviceScope.ServiceProvider, false);

                    Assert.NotNull(page.Service);
                    Assert.Null(page.Header.Service);
                    Assert.Null(page.Headers[0].Service);
                }
            }
        }

        [Fact]
        public void ApplyInjections_WithInnerModels()
        {
            var contentMetadata = metadataManager.GetMetadata<TestPageContent>();
            var services = new ServiceCollection().AddScoped<TestService>();
            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    var page = TestPageContent.Create("test", new PageHeaderContent(), new List<PageHeaderContent> { new PageHeaderContent() });

                    contentMetadata.ApplyInjections(page, serviceScope.ServiceProvider, true);

                    Assert.NotNull(page.Service);
                    Assert.NotNull(page.Header.Service);
                    Assert.NotNull(page.Headers[0].Service);
                }
            }
        }

        [Fact]
        public void Fields_DefaultSorting()
        {
            var contentMetadata = metadataManager.GetMetadata<ArticlePage>();

            var fields = contentMetadata.Fields.ToList();

            Assert.Equal("Title", fields[0].Name);
            Assert.Equal("Header", fields[1].Name);
            Assert.Equal("Headers", fields[2].Name);
            Assert.Equal("SubHeader", fields[3].Name);
        }

        //[Fact]
        //public void Fields_CustomSorting()
        //{
        //    var contentMetadata = metadataManager.GetMetadata<ArticlePage>();

        //    var fields = contentMetadata.Fields.ToList();
        //    fields.Sort();

        //    Assert.Equal("Title", fields[0].Name);
        //    Assert.Equal("SubHeader", fields[1].Name);
        //    Assert.Equal("Header", fields[2].Name);
        //    Assert.Equal("Headers", fields[3].Name);
        //}

        #endregion
    }
}