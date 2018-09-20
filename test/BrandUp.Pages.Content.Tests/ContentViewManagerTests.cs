using BrandUp.Pages.ContentModels;
using System.Linq;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentViewManagerTests
    {
        private readonly IContentViewManager manager;

        public ContentViewManagerTests()
        {
            var metadataManager = new ContentMetadataManager(new AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly }));

            manager = new ContentViewManager(metadataManager, new AttributesContentViewResolver());
        }

        [Fact]
        public void FindViewByName()
        {
            var viewName = "TestPage.Default";
            var view = manager.FindViewByName(viewName);
            Assert.NotNull(view);
            Assert.Equal(view.Name, viewName);
        }

        [Fact]
        public void GetViews()
        {
            var views = manager.GetViews(typeof(TestPageContent));
            Assert.NotNull(views);
            Assert.True(views.Count() > 0);
        }

        [Fact]
        public void GetContentView()
        {
            var view = manager.GetContentView(new TestPageContent { ViewName = "TestPage.Default" });
            Assert.NotNull(view);
        }

        [Fact]
        public void GetContentView_Default()
        {
            var view = manager.GetContentView(new TestPageContent { ViewName = null });
            Assert.NotNull(view);
        }
    }
}