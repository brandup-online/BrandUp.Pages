using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPagesTesting(this IServiceCollection services)
        {
            services.AddSingleton<FakePageHierarhyRepository>();

            services.AddSingleton<IPageEditorRepository, FakePageEditorRepository>();
            services.AddSingleton<IPageRepository, FakePageRepositiry>();
            services.AddSingleton<IPageCollectionRepository, FakePageCollectionRepositiry>();
            services.AddSingleton<IPageContentRepository, FakePageContentRepository>();
            services.AddSingleton<BrandUp.Pages.Files.IFileRepository, FakePageFileRepository>();
            services.AddSingleton<BrandUp.Pages.Administration.IAdministrationManager, BrandUp.Pages.Administration.FakeAdministrationManager>();
            services.AddSingleton<BrandUp.Pages.Views.IViewLocator, BrandUp.Pages.Views.FakeViewLocator>();

            return services;
        }
    }
}