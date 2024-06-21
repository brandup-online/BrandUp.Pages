using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Repositories;
using BrandUp.Pages.Testing.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPagesTesting(this IServiceCollection services)
        {
            services.AddSingleton<FakePageHierarhyRepository>();

            services.AddSingleton<IPageRepository, FakePageRepositiry>();
            services.AddSingleton<IPageCollectionRepository, FakePageCollectionRepositiry>();
            services.AddSingleton<IContentEditRepository, FakeContentEditRepository>();
            services.AddSingleton<IContentRepository, FakeContentRepository>();
            services.AddSingleton<BrandUp.Pages.Files.IFileRepository, FakePageFileRepository>();
            services.AddSingleton<BrandUp.Pages.Views.IViewLocator, BrandUp.Pages.Views.FakeViewLocator>();

            return services;
        }
    }
}