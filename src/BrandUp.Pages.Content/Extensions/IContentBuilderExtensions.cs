using BrandUp.Pages.Content.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.Pages.Content.Builder
{
    public static class IServiceCollectionExtensions
    {
        public static IContentBuilder AddManager<TEntry, TModel>(this IContentBuilder builder)
            where TEntry : class, IContentEntry
            where TModel : class
        {
            AddManager<ContentManager<TEntry, TModel>, TEntry, TModel>(builder);

            return builder;
        }

        public static IContentBuilder AddManager<TManager, TEntry, TModel>(this IContentBuilder builder)
            where TManager : class, IContentManager<TEntry, TModel>
            where TEntry : class, IContentEntry
            where TModel : class
        {
            builder.Services.AddScoped<IContentManager<TEntry, TModel>, TManager>();
            builder.Services.AddScoped(s => (TManager)s.GetRequiredService<IContentManager<TEntry, TModel>>());
            builder.Services.AddSingleton<IContentStore<TEntry>, MemoryContentStore<TEntry>>();

            return builder;
        }

        public static IContentBuilder AddStore<TEntry, TStore>(this IContentBuilder builder, ServiceLifetime serviceLifetime)
            where TEntry : class, IContentEntry
            where TStore : class, IContentStore<TEntry>
        {
            builder.Services.Add(new ServiceDescriptor(typeof(IContentStore<TEntry>), typeof(TStore), serviceLifetime));

            return builder;
        }

        public static IContentBuilder AddContentTypesFromAssemblies(this IContentBuilder builder, params Assembly[] assemblies)
        {
            builder.Services.AddSingleton<IContentTypeLocator>(new AssemblyContentTypeLocator(assemblies));
            return builder;
        }
    }
}