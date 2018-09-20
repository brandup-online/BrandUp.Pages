using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentEditorTests : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private readonly IContentMetadataManager metadataManager;
        private readonly IContentViewManager viewManager;

        public ContentEditorTests()
        {
            var services = new ServiceCollection();

            services.AddWebSiteCore()
                .UseContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .UseContentViewsFromAttributes();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            metadataManager = serviceScope.ServiceProvider.GetService<IContentMetadataManager>();
            viewManager = serviceScope.ServiceProvider.GetService<IContentViewManager>();
        }

        public void Dispose()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();
        }

        [Fact]
        public void Create()
        {
            var content = new TestPageContent();
            var explorer = ContentExplorer.Create(metadataManager, viewManager, content);

            var editor = new ContentEditor(explorer);
            Assert.Equal(editor.Explorer, explorer);
        }

        [Fact]
        public void SetFieldValue_Changed()
        {
            var content = new TestPageContent();
            var explorer = ContentExplorer.Create(metadataManager, viewManager, content);
            var editor = new ContentEditor(explorer);

            var isChanged = editor.SetValue("Title", "test");

            Assert.True(isChanged);
            Assert.Equal("test", content.Title);
        }

        [Fact]
        public void SetFieldValue_NotChanged()
        {
            var content = new TestPageContent { Title = "test" };
            var explorer = ContentExplorer.Create(metadataManager, viewManager, content);
            var editor = new ContentEditor(explorer);

            var isChanged = editor.SetValue("Title", "test");

            Assert.False(isChanged);
            Assert.Equal("test", content.Title);
        }
    }
}