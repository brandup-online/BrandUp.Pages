using BrandUp.Pages.Content.Items;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Content
{
    public static class IServiceProviderExtensions
    {
        public static IServiceCollection AddContentMappingProvider<TItem, TProvider>(this IServiceCollection services)
            where TItem : IItemContent
            where TProvider : class, IItemContentProvider<TItem>
        {
            ArgumentNullException.ThrowIfNull(services);

            var serviceKey = MappingHelper.GetServiceKey<TItem>();
            services.AddKeyedScoped<IItemContentProvider<TItem>, TProvider>(serviceKey);

            return services;
        }

        public static IItemContentProvider<TItem> GetContentMappingProvider<TItem>(this IServiceProvider serviceProvider)
            where TItem : IItemContent
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var serviceKey = MappingHelper.GetServiceKey<TItem>();

            return serviceProvider.GetRequiredKeyedService<IItemContentProvider<TItem>>(serviceKey);
        }
    }
}