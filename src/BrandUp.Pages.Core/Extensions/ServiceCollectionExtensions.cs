using BrandUp.Pages.Builder;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IPagesBuilder AddPages(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return new PagesBuilder(services);
        }

        public static IPagesBuilder AddPages(this IServiceCollection services, Action<PagesOptions> setupAction)
        {
            var webSiteBuilder = services.AddPages();
            webSiteBuilder.Services.Configure(setupAction);
            return webSiteBuilder;
        }
    }
}