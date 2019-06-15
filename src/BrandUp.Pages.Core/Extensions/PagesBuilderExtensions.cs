using BrandUp.Pages.Content.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.Pages.Builder
{
    public static class PagesBuilderExtensions
    {
        public static IPagesBuilder AddContentTypesFromAssemblies(this IPagesBuilder builder, params Assembly[] assemblies)
        {
            builder.Services.AddSingleton<IContentTypeLocator>(new AssemblyContentTypeResolver(assemblies));
            return builder;
        }

        public static IPagesBuilder AddAdministrationManager<T>(this IPagesBuilder builder, ServiceLifetime serviceLifetime)
            where T : Administration.IAdministrationManager
        {
            builder.Services.Add(new ServiceDescriptor(typeof(Administration.IAdministrationManager), typeof(T), serviceLifetime));

            return builder;
        }
    }
}