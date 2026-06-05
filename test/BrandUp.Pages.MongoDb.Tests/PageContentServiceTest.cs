using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.MongoDb.Tests.ContentModels;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.MongoDb.Tests
{
	[Collection(nameof(MongoDatabases))]
	public class PageContentServiceTest(MongoDbInstance fakeMongoDbInstance) : TestBase(fakeMongoDbInstance)
	{
		[Fact]
		public async Task SetContent_AfterSessionRemoved_ThrowsPageEditNotFound()
		{
			#region Prepare

			var pageCollectionService = Services.GetRequiredService<IPageCollectionService>();
			var pageService = Services.GetRequiredService<IPageService>();
			var pageContentService = Services.GetRequiredService<IPageContentService>();
			var pageMetadataManager = Services.GetRequiredService<IPageMetadataManager>();
			var pageContentType = pageMetadataManager.GetMetadata<TestPageContent>();

			var pageCollection = (await pageCollectionService.CreateCollectionAsync(websiteContext.Website.Id, "test", pageContentType.Name, PageSortMode.FirstOld)).Data!;
			var page = await pageService.CreatePageAsync(pageCollection, pageContentType.Name, "test", TestContext.Current.CancellationToken);

			#endregion

			var edit = await pageContentService.BeginEditAsync(page, TestContext.Current.CancellationToken);

			// Session removed (committed/discarded elsewhere) while the caller still holds editId.
			await pageContentService.DiscardEditAsync(edit, TestContext.Current.CancellationToken);

			var ex = await Assert.ThrowsAsync<PageEditNotFoundException>(
				() => pageContentService.SetContentAsync(edit, new TestPageContent { Title = "stale" }, TestContext.Current.CancellationToken));

			Assert.Equal(edit.Id, ex.EditId);
		}
	}
}
