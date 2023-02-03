using BrandUp.Pages.Metadata;
using BrandUp.Pages.MongoDb.Tests.ContentModels;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.MongoDb.Tests
{
    public class PageCollectionServiceTest : TestBase
    {
        [Fact]
        public async Task CreateCollection()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

            var createResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);

            Assert.True(createResult.IsSuccess);

            var pageCollection = createResult.Data;
            Assert.Null(pageCollection.PageId);
            Assert.Equal("test", pageCollection.WebsiteId);
            Assert.Equal(pageContentType.Name, pageCollection.PageTypeName);
            Assert.Equal("test", pageCollection.Title);
            Assert.Equal(PageSortMode.FirstOld, pageCollection.SortMode);
        }

        [Fact]
        public async Task CreateCollection_Child_Success()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageService = Services.GetRequiredService<IPageService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

            var createCollectionResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult.IsSuccess);

            var page = await pageService.CreatePageAsync(createCollectionResult.Data, pageContentType.Name, "test");
            await pageService.PublishPageAsync(page, "test");

            createCollectionResult = await pageCollectionService.CreateCollectionAsync(page, "test2", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult.IsSuccess);

            var pageCollection = createCollectionResult.Data;
            Assert.Equal(page.Id, pageCollection.PageId);
            Assert.Equal("test", pageCollection.WebsiteId);
            Assert.Equal(pageContentType.Name, pageCollection.PageTypeName);
            Assert.Equal("test2", pageCollection.Title);
            Assert.Equal(PageSortMode.FirstOld, pageCollection.SortMode);
        }

        [Fact]
        public async Task CreateCollection_Child_Error_RequirePublish()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageService = Services.GetRequiredService<IPageService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

            var createCollectionResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult.IsSuccess);

            var page = await pageService.CreatePageAsync(createCollectionResult.Data, pageContentType.Name, "test");

            createCollectionResult = await pageCollectionService.CreateCollectionAsync(page, "test2", pageContentType.Name, PageSortMode.FirstOld);
            Assert.False(createCollectionResult.IsSuccess);
        }

        [Fact]
        public async Task FindCollectiondById()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

            var createResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createResult.IsSuccess);

            var pageCollection = await pageCollectionService.FindCollectiondByIdAsync(createResult.Data.Id);
            Assert.NotNull(pageCollection);
            Assert.Equal(createResult.Data.Id, pageCollection.Id);
        }

        [Fact]
        public async Task ListCollections_Root_Empty()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();

            var collections = await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id);
            Assert.Empty(collections);
        }

        [Fact]
        public async Task ListCollections_Root_Single()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

            var createResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createResult.IsSuccess);

            var collections = await pageCollectionService.ListCollectionsAsync(websiteContext.Website.Id);
            Assert.Single(collections);

            Assert.Equal(createResult.Data.Id, collections.First().Id);
        }

        [Fact]
        public async Task ListCollections_Child_Empty()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageService = Services.GetRequiredService<IPageService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

            var createCollectionResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult.IsSuccess);

            var page = await pageService.CreatePageAsync(createCollectionResult.Data, pageContentType.Name, "test");
            await pageService.PublishPageAsync(page, "test");

            var collections = await pageCollectionService.ListCollectionsAsync(page);
            Assert.Empty(collections);
        }

        [Fact]
        public async Task ListCollections_Child_Single()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageService = Services.GetRequiredService<IPageService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

            var createCollectionResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult.IsSuccess);

            var page = await pageService.CreatePageAsync(createCollectionResult.Data, pageContentType.Name, "test");
            await pageService.PublishPageAsync(page, "test");

            createCollectionResult = await pageCollectionService.CreateCollectionAsync(page, "test2", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult.IsSuccess);

            var collections = await pageCollectionService.ListCollectionsAsync(page);
            Assert.Single(collections);

            Assert.Equal(createCollectionResult.Data.Id, collections.First().Id);
        }

        [Fact]
        public async Task FindCollections_Empty()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();
            var articleContentType = pageMetadataManager.GetMetadata<ArticlePageContent>();

            var createCollectionResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult.IsSuccess);

            var findCollections = await pageCollectionService.FindCollectionsAsync(websiteContext.Website.Id, articleContentType.Name);
            Assert.Empty(findCollections);
        }

        [Fact]
        public async Task FindCollections_NotDerived_Success()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();
            var articleContentType = pageMetadataManager.GetMetadata<ArticlePageContent>();

            var createCollectionResult1 = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test1", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult1.IsSuccess);
            var createCollectionResult2 = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test2", articleContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult2.IsSuccess);

            var findCollections = await pageCollectionService.FindCollectionsAsync(websiteContext.Website.Id, pageContentType.Name, null, false);
            Assert.Single(findCollections);
            Assert.Equal(createCollectionResult1.Data.Id, findCollections.First().Id);
        }

        [Fact]
        public async Task FindCollections_Derived_Success()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();
            var articleContentType = pageMetadataManager.GetMetadata<ArticlePageContent>();

            var createCollectionResult1 = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test1", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult1.IsSuccess);
            var createCollectionResult2 = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test2", articleContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult2.IsSuccess);

            var findCollections = await pageCollectionService.FindCollectionsAsync(websiteContext.Website.Id, pageContentType.Name, null, true);
            Assert.Equal(2, findCollections.ToList().Count);
        }

        [Fact]
        public async Task FindCollections_Text_Success()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();
            var articleContentType = pageMetadataManager.GetMetadata<ArticlePageContent>();

            var createCollectionResult1 = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test1", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult1.IsSuccess);
            var createCollectionResult2 = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test2", articleContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult2.IsSuccess);

            var findCollections = await pageCollectionService.FindCollectionsAsync(websiteContext.Website.Id, pageContentType.Name, "test1", true);
            Assert.Single(findCollections);
        }

        [Fact]
        public async Task FindCollections_Text_Empty()
        {
            var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
            var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
            var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();
            var articleContentType = pageMetadataManager.GetMetadata<ArticlePageContent>();

            var createCollectionResult1 = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test1", pageContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult1.IsSuccess);
            var createCollectionResult2 = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test2", articleContentType.Name, PageSortMode.FirstOld);
            Assert.True(createCollectionResult2.IsSuccess);

            var findCollections = await pageCollectionService.FindCollectionsAsync(websiteContext.Website.Id, pageContentType.Name, "test1", true);
            Assert.Single(findCollections);
        }
    }
}