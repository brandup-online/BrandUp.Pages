using BrandUp.Pages.Data;
using BrandUp.Pages.Data.Repositories;
using BrandUp.Pages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages.Builder
{
    public static class WebSiteBuilderExtensions
    {
        public static void UseMongoDbStore(this IPagesBuilder builder, Action<MongoDbOptions> setupAction)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            var services = builder.Services;

            services.AddSingleton<WebSiteContext>();

            AddMongoDbRepositories(services);

            services.Configure(setupAction);
        }

        public static void AddMongoDbRepositories(IServiceCollection services)
        {
            services.AddScoped<IPageCollectionRepositiry, PageCollectionRepository>();
            services.AddScoped<IPageRepositiry, PageRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IPageEditSessionRepository, PageEditSessionRepository>();
        }
    }

    public class MongoDbOptions
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}