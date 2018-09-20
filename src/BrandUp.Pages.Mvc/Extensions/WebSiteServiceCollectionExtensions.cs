using BrandUp.Pages;
using BrandUp.Pages.Content;
using BrandUp.Pages.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebSiteServiceCollectionExtensions
    {
        public static IWebSiteBuilder AddWebSite(this IMvcBuilder mvcBuilder)
        {
            if (mvcBuilder == null)
                throw new ArgumentNullException(nameof(mvcBuilder));

            mvcBuilder.Services.AddWebSiteCore();

            ConfigureDefaultFeatureProviders(mvcBuilder.PartManager);
            AddWebSiteServices(mvcBuilder.Services);

            return new WebSiteBuilder(mvcBuilder.Services);
        }

        private static void AddWebSiteServices(IServiceCollection services)
        {
            services.AddSingleton<IContentTypeResolver, ContentFeature>();
            services.AddSingleton<IContentViewResolver, MvcContentViewResolver>();
            services.AddSingleton<IContentDefaultModelResolver, MvcContentDefaultModelResolver>();
        }

        private static void ConfigureDefaultFeatureProviders(ApplicationPartManager manager)
        {
            if (!manager.FeatureProviders.OfType<ContentFeatureProvider>().Any())
                manager.FeatureProviders.Add(new ContentFeatureProvider());
        }

        public static IWebSiteBuilder AddWebSite(this IMvcBuilder mvcBuilder, Action<WebSiteOptions> setupAction)
        {
            var webSiteBuilder = mvcBuilder.AddWebSite();
            webSiteBuilder.Services.Configure(setupAction);
            return webSiteBuilder;
        }
    }
}