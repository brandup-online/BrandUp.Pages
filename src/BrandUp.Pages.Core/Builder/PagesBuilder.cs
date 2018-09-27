﻿using BrandUp.Pages.Content;
using BrandUp.Pages.Files;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Middlewares;
using BrandUp.Pages.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages.Builder
{
    public interface IPagesBuilder
    {
        IServiceCollection Services { get; }
    }

    public class PagesBuilder : IPagesBuilder
    {
        public PagesBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            AddCoreServices(services);
        }

        public IServiceCollection Services { get; }

        private static void AddCoreServices(IServiceCollection services)
        {
            services.AddSingleton<IContentMetadataManager, ContentMetadataManager>();
            services.AddSingleton<IContentViewManager, ContentViewManager>();

            services.AddSingleton<IPageMetadataManager, PageMetadataManager>();

            services.AddScoped<IPageCollectionService, PageCollectionService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IPageEditingService, PageEditingService>();

            services.AddTransient<PageMiddleware>();
        }
    }

    public class PagesOptions
    {

    }
}