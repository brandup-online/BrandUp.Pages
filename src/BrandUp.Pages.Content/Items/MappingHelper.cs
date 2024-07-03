using System.Collections.Concurrent;
using System.Reflection;

namespace BrandUp.Pages.Content.Items
{
    public static class MappingHelper
    {
        readonly static ConcurrentDictionary<Type, string> keys = [];
        readonly static ConcurrentDictionary<string, Type> types = [];

        public static string GetServiceKey<TItem>()
            where TItem : IItemContent
        {
            return keys.GetOrAdd(typeof(TItem), type =>
            {
                var attr = type.GetCustomAttribute<ContentItemAttribute>();
                if (attr == null)
                    throw new InvalidOperationException($"Type {type.AssemblyQualifiedName} is not defined attribute {typeof(ContentItemAttribute).FullName}.");

                if (!types.TryAdd(attr.Name, type))
                    throw new InvalidOperationException();

                return attr.Name;
            });
        }

        public static Type GetItemType(string name)
        {
            if (!types.TryGetValue(name, out var type))
                throw new ArgumentException($"Not found content item type by name \"{name}\".", nameof(name));

            return type;
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class ContentItemAttribute : Attribute
    {
        public string Name { get; }

        public ContentItemAttribute(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Require not null and not ampty name.", nameof(name));

            Name = name;
        }
    }
}