using BrandUp.Pages.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Builder
{
    public static class IRazorPagesBuilderExtensions
    {
        public const string ContentPageName = "/ContentPage";

        public static IPagesBuilder AddRazorContentPage(this IPagesBuilder builder)
        {
            var services = builder.Services;

            services.AddHttpContextAccessor();

            services.Configure<RazorPagesOptions>(options =>
            {
                options.Conventions.AddPageRoute(ContentPageName, "{**url}");
            });

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new ContentEditFilter());
            });

            services.AddTransient<Url.IPageLinkGenerator, Url.RazorPageLinkGenerator>();
            services.AddTransient<Files.IFileUrlGenerator, Url.FileUrlGenerator>();

            services.AddSingleton<Views.IViewLocator, Views.RazorViewLocator>();
            services.AddScoped<Views.IViewRenderService, Views.RazorViewRenderService>();

            return builder;
        }

        public static IPagesBuilder AddImageResizer<T>(this IPagesBuilder builder)
            where T : class, Images.IImageResizer
        {
            builder.Services.AddTransient<Images.IImageResizer, T>();

            return builder;
        }
    }
}