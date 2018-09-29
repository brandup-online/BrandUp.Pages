using BrandUp.Pages.Content.Views;
using BrandUp.Pages.Mvc;
using BrandUp.Pages.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages.Builder
{
    public static class PagesBuilderExtensions
    {
        public static IPagesBuilder UseMvcViews(this IPagesBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            AddServices(builder.Services);

            return builder;
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IContentViewResolver, MvcContentViewResolver>();
            services.AddTransient<IPageRenderer, MvcPageRenderer>();
        }
    }
}