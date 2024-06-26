using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.MongoDb.Tests.ContentModels;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.MongoDb.Tests
{
	public class PageServiceTest : TestBase
	{
		[Fact]
		public async Task Create()
		{
			#region Prepare

			var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
			var pageService = Services.GetRequiredService<IPageService>();
			var pageMetadataManager = Services.GetRequiredService<PageMetadataManager>();
			var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

			var createCollectionResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);
			var pageCollection = createCollectionResult.Data;

			#endregion

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
			#region Prepare

			var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
			var pageService = Services.GetRequiredService<IPageService>();
			var pageMetadataManager = Services.GetRequiredService<PageMetadataManager>();
			var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

			var createCollectionResult = await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld);
			var pageCollection = createCollectionResult.Data;

			#endregion

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