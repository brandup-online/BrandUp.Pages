using BrandUp.Pages.Content.Items;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Content
{
    public static class IServiceProviderExtensions
    {
        public static IServiceCollection AddContentMappingProvider<TItem, TProvider>(this IServiceCollection services)
            where TItem : IItemContent
            where TProvider : ItemContentProvider<TItem>
        {
            ArgumentNullException.ThrowIfNull(services);

            var serviceKey = MappingHelper.GetServiceKey<TItem>();
            services.AddKeyedScoped<ItemContentProvider<TItem>, TProvider>(serviceKey);

            return services;
        }

        public static ItemContentProvider<TItem> GetContentMappingProvider<TItem>(this IServiceProvider serviceProvider)
            where TItem : IItemContent
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var serviceKey = MappingHelper.GetServiceKey<TItem>();

            return serviceProvider.GetRequiredKeyedService<ItemContentProvider<TItem>>(serviceKey);
        }

        public static IItemContentProvider GetContentMappingProvider(this IServiceProvider serviceProvider, Type itemType)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var serviceKey = MappingHelper.GetServiceKey(itemType);

            var defType = typeof(ItemContentProvider<>);
            var itemProviderType = defType.MakeGenericType(itemType);

            return (IItemContentProvider)serviceProvider.GetRequiredKeyedService(itemProviderType, serviceKey);
        }

        internal static IItemContentEvents GetContentEvents(this IServiceProvider serviceProvider, string itemType)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(itemType);

            var type = MappingHelper.GetItemType(itemType);

            var defType = typeof(ItemContentProvider<>);
            var itemProviderType = defType.MakeGenericType(type);

            var itemContentEvents = serviceProvider.GetRequiredKeyedService(itemProviderType, itemType);
            if (itemContentEvents == null)
                throw new InvalidOperationException();
            return (IItemContentEvents)itemContentEvents;
        }
    }
}