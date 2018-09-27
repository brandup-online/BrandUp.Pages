using BrandUp.Pages.Content;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.Pages.Builder
{
    public static class PagesBuilderExtensions
    {
        public static IPagesBuilder AddContentTypesFromAssemblies(this IPagesBuilder builder, params Assembly[] assemblies)
        {
            builder.Services.AddSingleton<IContentTypeResolver>(new AssemblyContentTypeResolver(assemblies));
            return builder;
        }
    }
}