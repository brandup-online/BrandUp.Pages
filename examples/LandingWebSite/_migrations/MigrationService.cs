using BrandUp.Extensions.Migrations;

namespace LandingWebSite._migrations
{
    public class MigrationService(IServiceProvider serviceProvider) : IHostedService
    {
        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var migrationExecutor = scope.ServiceProvider.GetRequiredService<MigrationExecutor>();
            await migrationExecutor.UpAsync(cancellationToken);
        }

        async Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}