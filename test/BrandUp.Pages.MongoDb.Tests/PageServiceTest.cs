using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.MongoDb.Tests.ContentModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.MongoDb.Tests
{
    public class PageServiceTest : TestBase
    {
        [Fact]
        public async Task Create()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageService = Services.GetRequiredService<IPageService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

            var createCollectionResult = await pageCollectionService.CreateCollectionAsync("test", pageContentType.Name, PageSortMode.FirstOld, null);
            var pageCollection = createCollectionResult.Data;

            var page = await pageService.CreatePageAsync(pageCollection, pageContentType.Name, "test");

            Assert.NotNull(page);
            Assert.Equal("test", page.WebsiteId);
            Assert.Equal(pageContentType.Name, page.TypeName);
            Assert.Equal("test", page.Header);
            Assert.Equal(pageCollection.Id, page.OwnCollectionId);
            Assert.Equal(page.Id.ToString(), page.UrlPath);
            Assert.False(page.IsPublished);
        }

        [Fact]
        public async Task Create_WithObject()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageService = Services.GetRequiredService<IPageService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

            var createCollectionResult = await pageCollectionService.CreateCollectionAsync("test", pageContentType.Name, PageSortMode.FirstOld, null);
            var pageCollection = createCollectionResult.Data;

            var page = await pageService.CreatePageAsync(pageCollection, new TestPageContent { Title = "test5" });

            Assert.NotNull(page);
            Assert.Equal("test", page.WebsiteId);
            Assert.Equal(pageContentType.Name, page.TypeName);
            Assert.Equal("test5", page.Header);
            Assert.Equal(pageCollection.Id, page.OwnCollectionId);
            Assert.Equal(page.Id.ToString(), page.UrlPath);
            Assert.False(page.IsPublished);
        }
    }
}