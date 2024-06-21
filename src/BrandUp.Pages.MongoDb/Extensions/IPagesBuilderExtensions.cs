using BrandUp.Extensions.Migrations;
using BrandUp.MongoDB;
using BrandUp.Pages.Files;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Builder
{
    public static class IPagesBuilderExtensions
    {
        public static IPagesBuilder AddMongoDb(this IPagesBuilder builder, Action<MongoDbContextOptions> mongoConfigAction)
        {
            builder.Services
                .AddMongoDbContext<MongoDb.PagesDbContext>(mongoConfigAction)
                .UseCamelCaseElementName();

            return AddMongoDb<MongoDb.PagesDbContext>(builder);
        }

        public static IPagesBuilder AddMongoDb<TContext>(this IPagesBuilder builder)
            where TContext : MongoDbContext, MongoDb.IPagesDbContext
        {
            builder.Services.AddTransient<MongoDb.IPagesDbContext>(s => s.GetRequiredService<TContext>());

            builder.Services.Configure<MigrationOptions>(options =>
            {
                options.AddAssembly(typeof(MongoDb._migrations.SetupMigration).Assembly);
            });

            AddMongoDbRepositories(builder.Services);

            return builder;
        }

        public static void AddMongoDbRepositories(IServiceCollection services)
        {
            services.AddScoped<IPageCollectionRepository, PageCollectionRepository>();
            services.AddScoped<IPageRepository, PageRepository>();
            services.AddScoped<IFileRepository, PageFileRepository>();
            services.AddScoped<IContentEditRepository, ContentEditRepository>();
            services.AddScoped<IContentRepository, ContentRepository>();
        }
    }
}