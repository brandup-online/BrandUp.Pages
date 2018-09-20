using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Files;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Services;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IWebSiteBuilder AddWebSiteCore(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            AddWebSiteServices(services);

            return new WebSiteBuilder(services);
        }

        private static void AddWebSiteServices(IServiceCollection services)
        {
            services.AddSingleton<IContentMetadataManager, ContentMetadataManager>();
            services.AddSingleton<IContentViewManager, ContentViewManager>();

            services.AddSingleton<IPageMetadataManager, PageMetadataManager>();

            services.AddScoped<IPageCollectionService, PageCollectionService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IPageEditingService, PageEditingService>();
        }

        public static IWebSiteBuilder AddWebSiteCore(this IServiceCollection services, Action<WebSiteOptions> setupAction)
        {
            var webSiteBuilder = services.AddWebSiteCore();
            webSiteBuilder.Services.Configure(setupAction);
            return webSiteBuilder;
        }
    }
}