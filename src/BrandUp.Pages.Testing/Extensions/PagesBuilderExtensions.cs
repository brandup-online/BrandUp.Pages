using BrandUp.Pages.Content;
using BrandUp.Pages.Data.Repositories;
using BrandUp.Pages.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Builder
{
    public static class PagesBuilderExtensions
    {
        public static IPagesBuilder UseFakeViews(this IPagesBuilder builder)
        {
            builder.Services.AddSingleton<IContentViewResolver>(new AttributesContentViewResolver());
            return builder;
        }

        public static IPagesBuilder AddFakeRepositories(this IPagesBuilder builder)
        {
            builder.Services.AddSingleton<FakePageHierarhyRepository>();

            builder.Services.AddSingleton<IPageRepositiry, FakePageRepositiry>();
            builder.Services.AddSingleton<IPageCollectionRepositiry, FakePageCollectionRepositiry>();
            builder.Services.AddSingleton<IFileRepository, FakePageFileRepository>();

            return builder;
        }
    }
}