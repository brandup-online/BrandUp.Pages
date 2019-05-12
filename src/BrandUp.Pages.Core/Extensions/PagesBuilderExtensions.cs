using BrandUp.Pages.Content.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.Pages.Builder
{
    public static class PagesBuilderExtensions
    {
        public static IPagesBuilder AddContentTypesFromAssemblies(this IPagesBuilder builder, params Assembly[] assemblies)
        {
            builder.Services.AddSingleton<IContentLocator>(new AssemblyContentTypeResolver(assemblies));
            return builder;
        }
    }
}