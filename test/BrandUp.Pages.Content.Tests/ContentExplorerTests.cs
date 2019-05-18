using BrandUp.Pages.ContentModels;
using System.Collections.Generic;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentExplorerTests
    {
        private readonly IContentMetadataManager metadataManager;

        public ContentExplorerTests()
        {
            var contentTypeResolver = new Infrastructure.AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });

            metadataManager = new ContentMetadataManager(contentTypeResolver);
        }

        #region Test methods

        [Fact]
        public void Create_Root()
        {
            var content = new TestPageContent();
            var explorer = ContentExplorer.Create(metadataManager, content);

            Assert.NotNull(explorer);
            Assert.NotNull(explorer.Metadata);
            Assert.Null(explorer.Field);
            Assert.Equal(content, explorer.Model);
            Assert.Equal(string.Empty, explorer.ModelPath);
            Assert.Equal(content.Title, explorer.Title);
            Assert.Null(explorer.Root);
            Assert.Null(explorer.Parent);
            Assert.Equal(-1, explorer.Index);
            Assert.True(explorer.IsRoot);
        }

        [Fact]
        public void Create_SpecifyPath()
        {
            var content = new TestPageContent { Header = new PageHeaderContent() };
            var explorer = ContentExplorer.Create(metadataManager, content, "Header");

            Assert.NotNull(explorer);
            Assert.NotNull(explorer.Metadata);
            Assert.NotNull(explorer.Field);
            Assert.Equal(explorer.Model, content.Header);
            Assert.Equal("Header", explorer.ModelPath);
            Assert.NotEqual(explorer.Root, explorer);
            Assert.NotNull(explorer.Parent);
            Assert.Equal(explorer.Index, -1);
            Assert.False(explorer.IsRoot);
        }

        [Fact]
        public void Navigate_ContentField()
        {
            var content = new TestPageContent
            {
                Header = new PageHeaderContent()
            };
            var explorer = ContentExplorer.Create(metadataManager, content, "Header");

            Assert.NotNull(explorer);
        }

        [Fact]
        public void Navigate_ContentField_Null()
        {
            var content = new TestPageContent();
            var explorer = ContentExplorer.Create(metadataManager, content, "Header");

            Assert.Null(explorer);
        }

        [Fact]
        public void Navigate_ContentListField()
        {
            var content = new TestPageContent
            {
                Headers = new List<PageHeaderContent> { new PageHeaderContent() }
            };
            var explorer = ContentExplorer.Create(metadataManager, content, "Headers[0]");

            Assert.NotNull(explorer);
        }

        [Fact]
        public void Navigate_ContentListField_Null()
        {
            var content = new TestPageContent();
            var explorer = ContentExplorer.Create(metadataManager, content, "Headers[0]");

            Assert.Null(explorer);
        }

        #endregion
    }
}