using BrandUp.MongoDB;
using BrandUp.MongoDB.Testing;
using BrandUp.Pages.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrandUp.Pages.Tests.Infrastructure
{
	/// <summary>
	/// Boots the <see cref="LandingWebSite"/> host for integration tests, replacing the
	/// real MongoDB with an ephemeral in-memory server so no external database is required.
	/// </summary>
	public class PagesWebApplicationFactory : WebApplicationFactory<LandingWebSite.Program>
	{
		/// <summary>When set, access checks always pass — emulates an authenticated administrator.</summary>
		public bool GrantAdministration { get; set; }

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseEnvironment("Development");

			builder.ConfigureTestServices(services =>
			{
				// Replace the configured MongoDB client factory with an ephemeral server.
				services.RemoveAll<IMongoDbClientFactory>();
				services.AddEphemeralMongoDb();

				// Optionally bypass the [Administration] access filter.
				if (GrantAdministration)
					services.AddSingleton<IAccessProvider, GrantAllAccessProvider>();
			});
		}
	}

	/// <summary>Access provider that authorizes every request — used to exercise admin-gated endpoints.</summary>
	public sealed class GrantAllAccessProvider : IAccessProvider
	{
		public Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<string?>("test-admin");

		public Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult(true);
	}
}
