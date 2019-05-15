using BrandUp.Pages.Content.Files;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPagesTesting(this IServiceCollection services)
        {
            services.AddSingleton<FakePageHierarhyRepository>();

            services.AddSingleton<IPageRepositiry, FakePageRepositiry>();
            services.AddSingleton<IPageCollectionRepositiry, FakePageCollectionRepositiry>();
            services.AddSingleton<IFileRepository, FakePageFileRepository>();

            return services;
        }
    }
}