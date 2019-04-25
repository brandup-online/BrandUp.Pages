using BrandUp.MongoDB;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.MongoDb.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages.Builder
{
    public static class WebSiteBuilderExtensions
    {
        public static IPagesBuilder AddMongoDb(this IPagesBuilder builder, Action<IMongoDbContextBuilder> options)
        {
            builder.Services.AddMongoDbContext<MongoDb.PagesDbContext>(options);

            return AddMongoDb<MongoDb.PagesDbContext>(builder);
        }

        public static IPagesBuilder AddMongoDb<TContext>(this IPagesBuilder builder)
            where TContext : MongoDbContext, MongoDb.IPagesDbContext
        {
            builder.Services.AddMongoDbContextExension<TContext, MongoDb.IPagesDbContext>();

            AddMongoDbRepositories(builder.Services);

            return builder;
        }

        public static void AddMongoDbRepositories(IServiceCollection services)
        {
            services.AddScoped<IPageCollectionRepositiry, PageCollectionRepository>();
            services.AddScoped<IPageRepositiry, PageRepository>();
            services.AddScoped<Content.IFileRepository, FileRepository>();
            services.AddScoped<IPageEditSessionRepository, PageEditSessionRepository>();
        }
    }
}