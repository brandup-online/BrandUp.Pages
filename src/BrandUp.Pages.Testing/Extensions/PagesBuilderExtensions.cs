using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Builder
{
    public static class PagesBuilderExtensions
    {
        public static IPagesBuilder AddFakeRepositories(this IPagesBuilder builder)
        {
            builder.Services.AddSingleton<FakePageHierarhyRepository>();

            builder.Services.AddSingleton<IPageRepositiry, FakePageRepositiry>();
            builder.Services.AddSingleton<IPageCollectionRepositiry, FakePageCollectionRepositiry>();
            builder.Services.AddSingleton<Files.IFileRepository, FakePageFileRepository>();

            return builder;
        }
    }
}