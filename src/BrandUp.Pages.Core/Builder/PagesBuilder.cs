using BrandUp.Pages.Content;
using BrandUp.Pages.Identity;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Builder
{
    public interface IPagesBuilder
    {
        IServiceCollection Services { get; }
    }

    public class PagesBuilder : IPagesBuilder
    {
        public PagesBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            AddCoreServices(services);
        }

        public IServiceCollection Services { get; }

        static void AddCoreServices(IServiceCollection services)
        {
            services.AddSingleton<ContentMetadataManager>();
            services.AddScoped<ContentService>();

            services.AddSingleton<PageMetadataManager>();
            services.AddScoped<PageCollectionService>();
            services.AddScoped<PageService>();

            services.AddSingleton<Url.IPageUrlHelper, Url.PageUrlHelper>();
            services.AddTransient<Url.IPageUrlPathGenerator, Url.PageUrlPathGenerator>();

            services.AddScoped<Files.FileService>();

            services.AddSingleton<Content.Infrastructure.IContentTypeLocator>(new Content.Infrastructure.EmptyContentTypeLocator());
            services.AddScoped<Content.Infrastructure.IDefaultContentDataProvider, ViewDefaultContentDataProvider>();

            services.AddSingleton<IAccessProvider, EmptyAccessProvider>();
        }
    }
}