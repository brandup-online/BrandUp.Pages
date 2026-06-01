using BrandUp.Extensions.Migrations;
using BrandUp.MongoDB;
using BrandUp.Pages.Builder;
using BrandUp.Pages.MongoDb.Tests.ContentModels;
using BrandUp.MongoDB.Testing;
using BrandUp.Website;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb.Tests
{
    public abstract class TestBase(MongoDbInstance mongoDbInstance) : IAsyncLifetime
    {
        protected readonly IWebsiteContext websiteContext = new TestWebsiteContext("test", "test");
        ServiceProvider serviceProvider = null!;
        IServiceScope serviceScope = null!;

        public IServiceProvider Services => serviceScope.ServiceProvider;

        #region IAsyncLifetime members

        async ValueTask IAsyncLifetime.InitializeAsync()
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddSingleton(websiteContext);

            services.AddSingleton<IMongoDbClientFactory>(mongoDbInstance);

            services
                .AddPages()
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakes()
                .AddMongoDb(options =>
                {
                    options.DatabaseName = "Test";
                });

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
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await mongoDbInstance.CleanUpAsync();

            serviceScope.Dispose();
            await serviceProvider.DisposeAsync();
        }

        #endregion
    }

    [CollectionDefinition(nameof(MongoDatabases))]
    public class MongoDatabases : ICollectionFixture<MongoDbInstance> { }

    public class MongoDbInstance : IMongoDbClientFactory, IAsyncLifetime
    {
        ServiceProvider serviceProvider = null!;
        IMongoDbClientFactory factory = null!;
        IMongoClient client = null!;
        List<string> systemDatabaseNames = null!;

        public IMongoClient Client => client;

        #region IAsyncLifetime members

        async ValueTask IAsyncLifetime.InitializeAsync()
        {
            var services = new ServiceCollection();
            services.AddEphemeralMongoDb();

            serviceProvider = services.BuildServiceProvider();

            factory = serviceProvider.GetRequiredService<IMongoDbClientFactory>();
            client = factory.ResolveClient();

            systemDatabaseNames = await (await client.ListDatabaseNamesAsync()).ToListAsync();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await serviceProvider.DisposeAsync();
        }

        #endregion

        #region IMongoDbClientFactory members

        IMongoClient IMongoDbClientFactory.ResolveClient()
        {
            return client;
        }

        #endregion

        public async Task CleanUpAsync()
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var databaseNames = await (await client.ListDatabaseNamesAsync(new ListDatabaseNamesOptions { AuthorizedDatabases = true }, cancellation.Token)).ToListAsync(cancellation.Token);
            foreach (var dbName in databaseNames)
            {
                if (systemDatabaseNames.Contains(dbName, StringComparer.InvariantCultureIgnoreCase))
                    continue;

                await client.DropDatabaseAsync(dbName, cancellation.Token);
            }
        }
    }
}