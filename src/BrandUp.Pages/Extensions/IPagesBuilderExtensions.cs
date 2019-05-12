using Microsoft.AspNetCore.Mvc.RazorPages;
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
            builder.Services.Configure<RazorPagesOptions>(options =>
            {
                options.Conventions.AddPageRoute(Url.RazorPageLinkGenerator.RazorPagePath, "{*url}");
            });

            builder.Services.AddTransient<Url.IPageLinkGenerator, Url.RazorPageLinkGenerator>();

            builder.Services.AddSingleton<Views.IViewLocator, Views.ViewLocator>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure(optionAction);

            return builder;
        }
    }
}