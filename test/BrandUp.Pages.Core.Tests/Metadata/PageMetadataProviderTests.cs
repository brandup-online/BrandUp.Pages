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
            var contentTypeResolver = new AssemblyContentTypeResolver(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });
            var contentMetadataManager = new ContentMetadataManager(contentTypeResolver);

            pageMetadataManager = new PageMetadataManager(contentMetadataManager);
        }

        [Fact]
        public void IsInheritedOf()
        {
            var basePageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));
            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(ArticlePageContent));

            Assert.True(pageType.IsInheritedOf(basePageType));
        }
    }
}