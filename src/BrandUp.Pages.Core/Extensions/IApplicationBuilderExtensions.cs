using BrandUp.Pages.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class IApplicationBuilderExtensions
    {
        public static void UsePages(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<PageMiddleware>();
        }
    }
}