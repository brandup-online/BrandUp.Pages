using BrandUp.Pages;
using BrandUp.Pages.Builder;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IPagesBuilder AddPages(this IServiceCollection services)
        {
            return AddPages(services, options => { });
        }

        public static IPagesBuilder AddPages(this IServiceCollection services, Action<PagesOptions> setupAction)
        {
            services.Configure(setupAction);

            return new PagesBuilder(services);
        }
    }
}