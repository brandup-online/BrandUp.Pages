using BrandUp.Extensions.Migrations;

namespace LandingWebSite._migrations
{
	public class MigrationService(IServiceProvider serviceProvider) : Microsoft.Extensions.Hosting.IHostedService
	{
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await using var scope = serviceProvider.CreateAsyncScope();

			var migrationExecutor = scope.ServiceProvider.GetRequiredService<MigrationExecutor>();
			await migrationExecutor.UpAsync(cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await Task.CompletedTask;
		}
	}
}