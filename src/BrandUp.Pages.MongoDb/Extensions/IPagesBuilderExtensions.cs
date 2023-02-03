using BrandUp.Extensions.Migrations;
using BrandUp.MongoDB;
using BrandUp.Pages.Builder;
using BrandUp.Pages.Files;
using BrandUp.Pages.MongoDb.Repositories;
using BrandUp.Pages.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    public static class IPagesBuilderExtensions
    {
        public static IPagesBuilder AddMongoDb(this IPagesBuilder builder, Action<IMongoDbContextBuilder> options)
        {
            builder.Services.AddMongoDbContext<MongoDb.PagesDbContext>(options);

            return AddMongoDb<MongoDb.PagesDbContext>(builder);
        }

        public static IPagesBuilder AddMongoDb(this IPagesBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddMongoDbContext<MongoDb.PagesDbContext>(configuration);

            return AddMongoDb<MongoDb.PagesDbContext>(builder);
        }

        public static IPagesBuilder AddMongoDb<TContext>(this IPagesBuilder builder)
            where TContext : MongoDbContext, MongoDb.IPagesDbContext
        {
            builder.Services.AddMongoDbContextExension<TContext, MongoDb.IPagesDbContext>();
            builder.Services.Configure<MigrationOptions>(options =>
            {
                options.AddAssembly(typeof(MongoDb._migrations.SetupMigration).Assembly);
            });

            AddMongoDbRepositories(builder.Services);

            return builder;
        }

        public static void AddMongoDbRepositories(IServiceCollection services)
        {
            services.AddSingleton<IPageCollectionRepository, PageCollectionRepository>();
            services.AddSingleton<IPageRepository, PageRepository>();
            services.AddSingleton<IFileRepository, PageFileRepository>();
            services.AddSingleton<IPageContentRepository, PageContentRepository>();
        }
    }
}