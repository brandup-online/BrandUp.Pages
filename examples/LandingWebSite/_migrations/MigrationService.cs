using System.Threading;
using System.Threading.Tasks;

namespace LandingWebSite._migrations
{
    public class MigrationService : Microsoft.Extensions.Hosting.IHostedService
    {
        private readonly BrandUp.Extensions.Migrations.MigrationExecutor migrationExecutor;

        public MigrationService(BrandUp.Extensions.Migrations.MigrationExecutor migrationExecutor)
        {
            this.migrationExecutor = migrationExecutor ?? throw new System.ArgumentNullException(nameof(migrationExecutor));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await migrationExecutor.UpAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}