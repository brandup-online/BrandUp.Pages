using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages.Builder
{
    public static class IPagesBuilderExtensions
    {
        public static IPagesBuilder AddRazorContentPage(this IPagesBuilder builder)
        {
            return AddRazorContentPage(builder, options => { });
        }

        public static IPagesBuilder AddRazorContentPage(this IPagesBuilder builder, Action<RazorContentPageOptions> optionAction)
        {
            var services = builder.Services;

            services.AddHttpContextAccessor();

            services.Configure<RazorPagesOptions>(options =>
            {
                options.Conventions.AddPageRoute(Url.RazorPageLinkGenerator.RazorPagePath, "{**url}");
            });

            services.AddTransient<Url.IPageLinkGenerator, Url.RazorPageLinkGenerator>();
            services.AddTransient<Files.IFileUrlGenerator, Url.FileUrlGenerator>();

            services.AddSingleton<Views.IViewLocator, Views.RazorViewLocator>();
            services.AddScoped<Views.IViewRenderService, Views.RazorViewRenderService>();

            services.Configure(optionAction);

            services.AddTransient<ITagHelperComponent, TagHelpers.EmbeddingTagHelperComponent>();

            builder.AddAdministrationManager<Administration.DefaultAdministrationManager>(ServiceLifetime.Transient);

            return builder;
        }

        public static IPagesBuilder AddNavigationProvider<T>(this IPagesBuilder builder)
            where T : class, IPageNavigationProvider
        {
            builder.Services.AddTransient<IPageNavigationProvider, T>();

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