using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Builder
{
    public static class IPagesBuilderExtensions
    {
        public static IPagesBuilder AddRazorContentPage(this IPagesBuilder builder)
        {
            return AddRazorContentPage(builder, options => { });
        }

        public static IPagesBuilder AddRazorContentPage(this IPagesBuilder builder, Action<ContentPageOptions> optionAction)
        {
            var services = builder.Services;

            var contentPageOptions = new ContentPageOptions();
            optionAction?.Invoke(contentPageOptions);
            services.Configure(optionAction);

            services.AddHttpContextAccessor();

            services.Configure<RazorPagesOptions>(options =>
            {
                options.Conventions.AddPageRoute(contentPageOptions.ContentPageName, "{**url}");
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