using System.Collections.Concurrent;

namespace BrandUp.Pages.Content.Items
{
    internal static class MappingHelper
    {
        readonly static ConcurrentDictionary<Type, string> keys = [];

        public static string GetServiceKey<TItem>()
            where TItem : IItemContent
        {
            return keys.GetOrAdd(typeof(TItem), type => type.FullName);
        }
    }
}