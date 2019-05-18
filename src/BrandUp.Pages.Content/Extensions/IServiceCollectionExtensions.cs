using BrandUp.Pages.Content.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IContentBuilder AddContent(this IServiceCollection services)
        {
            return new ContentBuilder(services);
        }
    }
}