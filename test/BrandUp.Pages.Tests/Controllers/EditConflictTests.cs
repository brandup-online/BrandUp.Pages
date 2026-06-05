using System.Net;
using System.Net.Http.Json;
using BrandUp.Pages.Tests.Infrastructure;

namespace BrandUp.Pages.Tests.Controllers
{
	/// <summary>
	/// A well-formed editId pointing at a missing edit session is a stale-edit conflict,
	/// so content endpoints must answer 409 Conflict (not 400 / 404 / 500), which the
	/// editor UI handles by reloading.
	/// </summary>
	public class EditConflictTests
	{
		[Fact]
		public async Task FieldSave_UnknownEditSession_Conflict()
		{
			using var factory = new PagesWebApplicationFactory { GrantAdministration = true };
			using var client = factory.CreateClient();

			var url = $"/brandup.pages/content/html?editId={Guid.NewGuid()}&path=&field=Text";
			var response = await client.PostAsJsonAsync(url, "value", TestContext.Current.CancellationToken);

			Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
		}

		[Fact]
		public async Task GetForm_UnknownEditSession_Conflict()
		{
			using var factory = new PagesWebApplicationFactory { GrantAdministration = true };
			using var client = factory.CreateClient();

			var url = $"/brandup.pages/page/content/form?editId={Guid.NewGuid()}&modelPath=";
			var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

			Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
		}
	}
}
