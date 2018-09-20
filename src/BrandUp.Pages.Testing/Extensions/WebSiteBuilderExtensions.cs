using BrandUp.Pages.Content;
using BrandUp.Pages.Data.Repositories;
using BrandUp.Pages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.Pages.Testing
{
    public static class WebSiteBuilderExtensions
    {
        public static IWebSiteBuilder UseContentTypesFromAssemblies(this IWebSiteBuilder builder, params Assembly[] assemblies)
        {
            builder.Services.AddSingleton<IContentTypeResolver>(new AssemblyContentTypeResolver(assemblies));
            return builder;
        }
        public static IWebSiteBuilder UseContentViewsFromAttributes(this IWebSiteBuilder builder)
        {
            builder.Services.AddSingleton<IContentViewResolver>(new AttributesContentViewResolver());
            return builder;
        }
        public static IWebSiteBuilder UseFakeRepositories(this IWebSiteBuilder builder)
        {
            builder.Services.AddSingleton<FakePageHierarhyRepository>();

            builder.Services.AddSingleton<IPageRepositiry, FakePageRepositiry>();
            builder.Services.AddSingleton<IPageCollectionRepositiry, FakePageCollectionRepositiry>();
            builder.Services.AddSingleton<IFileRepository, FakePageFileRepository>();

            return builder;
        }
    }
}