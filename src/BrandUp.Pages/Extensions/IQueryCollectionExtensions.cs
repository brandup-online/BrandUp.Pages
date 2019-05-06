using Microsoft.AspNetCore.Http;

namespace BrandUp.Pages
{
    public static class IQueryCollectionExtensions
    {
        public static bool TryGetValue(this IQueryCollection collection, string name, out string value)
        {
            if (!collection.TryGetValue(name, out Microsoft.Extensions.Primitives.StringValues values))
            {
                value = default;
                return false;
            }

            value = values[0];
            return true;
        }
    }
}