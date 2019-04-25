using BrandUp.Pages.Data.Repositories;
using BrandUp.Pages.Interfaces;
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
            builder.Services.AddSingleton<Content.IFileRepository, FakePageFileRepository>();

            return builder;
        }
    }
}