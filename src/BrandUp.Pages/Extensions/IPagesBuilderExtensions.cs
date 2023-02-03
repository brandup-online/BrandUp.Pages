using BrandUp.Pages.Builder;
using BrandUp.Pages.Items;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    public static class IPagesBuilderExtensions
    {
        /// <summary>
        /// Маппинг корневых страниц сайта.
        /// </summary>
        /// <param name="pageName">Имя страницы.</param>
        public static IPagesBuilder AddRootPages(this IPagesBuilder builder, string pageName = "/Content")
        {
            if (pageName == null)
                throw new ArgumentNullException(nameof(pageName));

            var services = builder.Services;

            services.Configure<RootPageOptions>(options =>
            {
                options.ContentPageName = pageName;
            });

            services.PostConfigureAll<RazorPagesOptions>(options =>
            {
                options.Conventions.AddPageRoute(pageName, "{**url}");
            });

            return builder;
        }

        /// <summary>
        /// Маппинг страниц элементов с типом <see cref="TItem"/>.
        /// </summary>
        /// <typeparam name="TItem">Тип элемента страницы.</typeparam>
        /// <typeparam name="TStore">Хранилище элементов страницы.</typeparam>
        public static IPagesBuilder AddItemPages<TItem, TStore>(this IPagesBuilder builder)
            where TItem : class
            where TStore : class, IItemProvider<TItem>
        {
            var services = builder.Services;
            var itemTypeName = typeof(TItem).FullName;

            services.Configure<ItemPageOptions>(itemTypeName, options =>
            {
                options.ItemProviderType = typeof(IItemProvider<TItem>);
            });

            services.AddScoped<IItemProvider<TItem>, TStore>();

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