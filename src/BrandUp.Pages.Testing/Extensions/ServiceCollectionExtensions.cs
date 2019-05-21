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
            services.AddSingleton<BrandUp.Pages.Files.IFileRepository, FakePageFileRepository>();

            return services;
        }
    }
}