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

        public static IPagesBuilder AddUserProvider<T>(this IPagesBuilder builder, ServiceLifetime serviceLifetime)
            where T : Identity.IUserProvider
        {
            builder.Services.Add(new ServiceDescriptor(typeof(Identity.IUserProvider), typeof(T), serviceLifetime));

            return builder;
        }

        public static IPagesBuilder AddUserAccessProvider<T>(this IPagesBuilder builder, ServiceLifetime serviceLifetime)
            where T : Identity.IAccessProvider
        {
            builder.Services.Add(new ServiceDescriptor(typeof(Identity.IAccessProvider), typeof(T), serviceLifetime));

            return builder;
        }
    }
}