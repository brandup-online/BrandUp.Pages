using BrandUp.Pages.Content;
using BrandUp.Pages.Identity;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
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

            services.AddSingleton<IPageMetadataManager, PageMetadataManager>();
            services.AddScoped<IPageCollectionService, PageCollectionService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IPageContentService, PageContentService>();

            services.AddSingleton<Url.IPageUrlHelper, Url.PageUrlHelper>();
            services.AddTransient<Url.IPageUrlPathGenerator, Url.PageUrlPathGenerator>();

            services.AddScoped<Files.FileService>();

            services.AddSingleton<Content.Infrastructure.IContentTypeLocator>(new Content.Infrastructure.EmptyContentTypeLocator());

            services.AddSingleton<IAccessProvider, EmptyAccessProvider>();
        }
    }
}