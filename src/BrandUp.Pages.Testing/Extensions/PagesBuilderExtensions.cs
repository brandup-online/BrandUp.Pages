using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Builder
{
    public static class PagesBuilderExtensions
    {
        public static IPagesBuilder AddFakeRepositories(this IPagesBuilder builder)
        {
            builder.Services.AddPagesTesting();

            return builder;
        }
    }
}