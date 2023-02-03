using BrandUp.Pages;
using BrandUp.Pages.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IPagesBuilder AddPagesCore(this IServiceCollection services, Action<PagesOptions> setupAction = null)
        {
            services.Configure(setupAction ?? new Action<PagesOptions>(_ => { }));

            return new PagesBuilder(services);
        }
    }
}