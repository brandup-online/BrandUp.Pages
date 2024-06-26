using BrandUp.Pages.Content.Fakes;
using BrandUp.Pages.ContentModels;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Content.Fields
{
    public class ImageFieldTests : IAsyncLifetime
    {
        private ServiceProvider serviceProvider;
        private IServiceScope serviceScope;
        private ContentMetadataManager metadataManager;
        private IImageField field;

        #region IAsyncLifetime members

        Task IAsyncLifetime.InitializeAsync()
        {
            var contentTypeResolver = new Infrastructure.AssemblyContentTypeLocator([typeof(TestPageContent).Assembly]);
            var defaultContentDataProvider = new FakeDefaultContentDataProvider();

            var services = new ServiceCollection();

            services.AddSingleton<ContentMetadataManager, ContentMetadataManager>();
            services.AddSingleton<Infrastructure.IContentTypeLocator>(contentTypeResolver);
            services.AddSingleton<Infrastructure.IDefaultContentDataProvider>(defaultContentDataProvider);
            services.AddSingleton<Files.IFileUrlGenerator, Files.FakeFileUrlGenerator>();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            metadataManager = serviceScope.ServiceProvider.GetRequiredService<ContentMetadataManager>();

            var metadataProvider = metadataManager.GetMetadata<TestContent>();
            if (!metadataProvider.TryGetField("Image", out field))
                throw new System.Exception();

            return Task.CompletedTask;
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();

            return Task.CompletedTask;
        }

        #endregion

        #region Test methods

        [Fact]
        public void GetModelValue()
        {
            var content = new TestContent { Image = "url(http://test/test.jpg)" };
            var value = field.GetModelValue(content);

            Assert.Equal(content.Image, value);
        }

        [Fact]
        public void HasValue()
        {
            var content = new TestContent { Image = "url(http://test/test.jpg)" };
            var value = field.GetModelValue(content);

            Assert.True(field.HasValue(value));
        }

        [Fact]
        public void HasValue_False()
        {
            var content = new TestContent();
            var value = field.GetModelValue(content);

            Assert.False(field.HasValue(value));
        }

        [Fact]
        public void CompareValues()
        {
            var content = new TestContent { Image = "url(http://test/test.jpg)" };
            var value = field.GetModelValue(content);

            Assert.True(field.CompareValues(value, content.Image));
        }

        [Fact]
        public void SetModelValue()
        {
            var content = new TestContent { Image = "url(http://test/test.jpg)" };

            ImageValue newValue = "url(http://test/test2.jpg)";
            field.SetModelValue(content, newValue);

            Assert.Equal(newValue, content.Image);
        }

        [Fact]
        public void GetFormOptions()
        {
            var formOptions = field.GetFormOptions(serviceScope.ServiceProvider);

            Assert.Null(formOptions);
        }

        [Fact]
        public async Task GetFormValue()
        {
            var content = new TestContent { Image = "url(http://test/test.jpg)" };

            var modelValue = field.GetModelValue(content);
            var formValue = (ImageFieldFormValue)await field.GetFormValueAsync(modelValue, serviceScope.ServiceProvider);

            Assert.Equal(content.Image.Value, formValue.Value);
            Assert.Equal(content.Image.ValueType, formValue.ValueType);
            Assert.NotNull(formValue.PreviewUrl);
        }

        #endregion
    }
}