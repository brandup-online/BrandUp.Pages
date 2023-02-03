using BrandUp.Pages.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    public static class IServiceCollectionExtensions
    {
        public static IPagesBuilder AddPages(this IServiceCollection services, Action<PagesOptions> setupAction = null)
        {
            var pagesBuilder = services.AddPagesCore(setupAction);

            services.AddHttpContextAccessor();

            services.AddTransient<Url.IPageLinkGenerator, Url.PageLinkGenerator>();
            services.AddTransient<Files.IFileUrlGenerator, Url.FileUrlGenerator>();

            services.AddSingleton<Views.IViewLocator, Views.RazorViewLocator>();
            services.AddScoped<Views.IViewRenderService, Views.RazorViewRenderService>();

            return pagesBuilder;
        }
    }
}