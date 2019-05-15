﻿using Microsoft.AspNetCore.Mvc.RazorPages;
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
                options.Conventions.AddPageRoute(Url.RazorPageLinkGenerator.RazorPagePath, "{*url}");
            });

            services.AddTransient<Url.IPageLinkGenerator, Url.RazorPageLinkGenerator>();
            services.AddSingleton<Views.IViewLocator, Views.ViewLocator>();
            services.AddSingleton<Views.IViewRenderService, Views.ViewRenderService>();

            services.Configure(optionAction);

            builder.Services.AddTransient<ITagHelperComponent, TagHelpers.EmbeddingTagHelperComponent>();
            builder.Services.AddTransient<Administration.IAdministrationManager, Administration.DefaultAdministrationManager>();

            return builder;
        }
    }
}