﻿using BrandUp.Pages.ContentModels;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentExplorerTests
    {
        private readonly IContentMetadataManager metadataManager;
        private readonly IContentViewManager viewManager;

        public ContentExplorerTests()
        {
            metadataManager = new ContentMetadataManager(new AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly }));
            viewManager = new ContentViewManager(metadataManager, new AttributesContentViewResolver());
        }

        [Fact]
        public void Create_Root()
        {
            var content = new TestPageContent();
            var explorer = ContentExplorer.Create(metadataManager, viewManager, content);

            Assert.NotNull(explorer);
            Assert.Equal(explorer.MetadataManager, metadataManager);
            Assert.Equal(explorer.ViewManager, viewManager);
            Assert.NotNull(explorer.Metadata);
            Assert.Null(explorer.Field);
            Assert.Equal(explorer.Content, content);
            Assert.Equal(explorer.Path, string.Empty);
            Assert.Null(explorer.Root);
            Assert.Null(explorer.Parent);
            Assert.Equal(explorer.Index, -1);
            Assert.True(explorer.IsRoot);
        }

        [Fact]
        public void Create_SpecifyPath()
        {
            var content = new TestPageContent { Header = new PageHeaderContent() };
            var explorer = ContentExplorer.Create(metadataManager, viewManager, content, "Header");

            Assert.NotNull(explorer);
            Assert.Equal(explorer.MetadataManager, metadataManager);
            Assert.Equal(explorer.ViewManager, viewManager);
            Assert.NotNull(explorer.Metadata);
            Assert.NotNull(explorer.Field);
            Assert.Equal(explorer.Content, content.Header);
            Assert.Equal("Header", explorer.Path);
            Assert.NotEqual(explorer.Root, explorer);
            Assert.NotNull(explorer.Parent);
            Assert.Equal(explorer.Index, -1);
            Assert.False(explorer.IsRoot);
        }

        [Fact]
        public void Navigate_ContentField_Null()
        {
            var content = new TestPageContent();
            var explorer = ContentExplorer.Create(metadataManager, viewManager, content, "Header");

            Assert.Null(explorer);
        }

        [Fact]
        public void Navigate_ContentListField_Null()
        {
            var content = new TestPageContent();
            var explorer = ContentExplorer.Create(metadataManager, viewManager, content, "Headers[0]");

            Assert.Null(explorer);
        }
    }
}