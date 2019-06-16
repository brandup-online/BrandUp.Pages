using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Builder
{
    public static class PagesBuilderExtensions
    {
        public static IPagesBuilder AddFakes(this IPagesBuilder builder)
        {
            builder.Services.AddPagesTesting();

            builder
                .AddUserProvider<Identity.FakeUserProvider>(ServiceLifetime.Singleton)
                .AddUserAccessProvider<Identity.FakeAccessProvider>(ServiceLifetime.Scoped);

            return builder;
        }
    }
}