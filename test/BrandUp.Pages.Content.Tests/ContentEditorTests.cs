using BrandUp.Pages.ContentModels;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentEditorTests
    {
        private readonly IContentMetadataManager metadataManager;
        private readonly IContentViewManager viewManager;

        public ContentEditorTests()
        {
            metadataManager = new ContentMetadataManager(new AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly }));
            viewManager = new ContentViewManager(metadataManager, new AttributesContentViewResolver());
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