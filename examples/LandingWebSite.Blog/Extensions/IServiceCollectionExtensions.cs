using BrandUp.MongoDB;
using LandingWebSite.Blog.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LandingWebSite.Blog
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddBlog<TContext>(this IServiceCollection services)
            where TContext : MongoDbContext, IBlogContext
        {
            services.AddMongoDbContextExension<TContext, IBlogContext>();
            services.AddScoped<IBlogPostRepository, BlogPostRepository>();
            services.AddScoped<BlogService>();

            return services;
        }
    }
}