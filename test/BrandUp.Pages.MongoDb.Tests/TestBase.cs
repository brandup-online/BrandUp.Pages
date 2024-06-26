using BrandUp.Extensions.Migrations;
using BrandUp.MongoDB;
using BrandUp.Pages.Builder;
using BrandUp.Pages.MongoDb.Tests.ContentModels;
using BrandUp.Website;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.MongoDb.Tests
{
    public abstract class TestBase : IAsyncLifetime
    {
        ServiceProvider serviceProvider;
        IServiceScope serviceScope;
        protected readonly IWebsiteContext websiteContext;

        public TestBase()
        {
            websiteContext = new TestWebsiteContext("test", "test");
        }

        public IServiceProvider Services => serviceScope.ServiceProvider;

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddSingleton(websiteContext);

            services
                .AddPages()
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakes()
                .AddMongoDb(options =>
                {
                    options.DatabaseName = "Test";
                });

            services.AddTestMongoDb();

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
