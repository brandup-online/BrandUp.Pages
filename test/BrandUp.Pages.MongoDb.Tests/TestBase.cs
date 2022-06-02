using BrandUp.Extensions.Migrations;
using BrandUp.MongoDB;
using BrandUp.Pages.Builder;
using BrandUp.Pages.MongoDb.Tests.ContentModels;
using BrandUp.Website;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.MongoDb.Tests
{
    public abstract class TestBase : IAsyncLifetime
    {
        private ServiceProvider serviceProvider;
        private IServiceScope serviceScope;

        public IServiceProvider Services => serviceScope.ServiceProvider;

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddSingleton<IWebsiteContext>(new TestWebsiteContext("test", "test"));

            services.AddPages()
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakes()
                .AddMongoDb(options =>
                {
                    options.DatabaseName = "Test";
                    options.UseCamelCaseElementName();
                });

            services.AddMongo2GoDbClientFactory();

            services.AddMigrations(options =>
            {
                options.AddAssembly(typeof(_migrations.SetupMigration).Assembly);
            });
            services.AddSingleton<IMigrationState, Helpers.MigrationState>();

            serviceProvider = services.BuildServiceProvider();

            using var migrateScope = serviceProvider.CreateScope();
            var migrationExecutor = migrateScope.ServiceProvider.GetRequiredService<MigrationExecutor>();
            await migrationExecutor.UpAsync();

            serviceScope = serviceProvider.CreateScope();
        }
        async Task IAsyncLifetime.DisposeAsync()
        {
            serviceScope.Dispose();
            await serviceProvider.DisposeAsync();
        }

        #endregion
    }
}
