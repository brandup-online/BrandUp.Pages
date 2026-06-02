using System.Net;
using BrandUp.Pages.Tests.Infrastructure;

namespace BrandUp.Pages.Tests.Controllers
{
	/// <summary>
	/// Verifies that the <c>[Administration]</c>-gated API controllers reject anonymous
	/// requests and serve content once access is granted.
	/// </summary>
	public class AdministrationAccessTests
	{
		[Theory]
		[InlineData("/brandup.pages/pageType")]
		[InlineData("/brandup.pages/page?collectionId=" + "00000000-0000-0000-0000-000000000000")]
		[InlineData("/brandup.pages/collection")]
		public async Task AdminApi_Anonymous_Unauthorized(string url)
		{
			using var factory = new PagesWebApplicationFactory { GrantAdministration = false };
			using var client = factory.CreateClient();

			var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		[Fact]
		public async Task PageTypeList_WithAccess_ReturnsJson()
		{
			using var factory = new PagesWebApplicationFactory { GrantAdministration = true };
			using var client = factory.CreateClient();

			var response = await client.GetAsync("/brandup.pages/pageType", TestContext.Current.CancellationToken);

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

			var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
			Assert.StartsWith("[", json.TrimStart());
		}

		[Fact]
		public async Task CollectionList_WithAccess_ReturnsOk()
		{
			using var factory = new PagesWebApplicationFactory { GrantAdministration = true };
			using var client = factory.CreateClient();

			var response = await client.GetAsync("/brandup.pages/collection", TestContext.Current.CancellationToken);

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		[Fact]
		public async Task FileController_UnknownFile_NotFound()
		{
			// FileController is public (no [Administration]); an unknown id must return 404.
			using var factory = new PagesWebApplicationFactory();
			using var client = factory.CreateClient();

			var response = await client.GetAsync($"/_file/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}
	}
}
