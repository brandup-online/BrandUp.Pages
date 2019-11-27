using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Infrastructure;
using BrandUp.Pages.ContentModels;
using Xunit;

namespace BrandUp.Pages.Metadata
{
    public class PageMetadataProviderTests
    {
        private readonly PageMetadataManager pageMetadataManager;

        public PageMetadataProviderTests()
        {
            var contentTypeResolver = new AssemblyContentTypeLocator(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });
            var contentMetadataManager = new ContentMetadataManager(contentTypeResolver);

            pageMetadataManager = new PageMetadataManager(contentMetadataManager);
        }

        [Fact]
        public void IsInheritedOf()
        {
            var basePageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(ArticlePageContent));

            Assert.True(pageType.IsInherited(basePageType));
        }

        [Fact]
        public void GetPageTitle()
        {
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var pageContent = new TestPageContent { Title = "test" };

            var pageTitle = pageType.GetPageHeader(pageContent);

            Assert.Equal(pageContent.Title, pageTitle);
        }

        [Fact]
        public void CreatePageModel()
        {
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var pageContent = (TestPageContent)pageType.CreatePageModel();

            Assert.Equal(new TestPageContent().Title, pageContent.Title);
        }
    }
}