using BrandUp.Pages.Content;
using BrandUp.Pages.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class HtmlHelperExtensions
    {
        public static object GetWebSitePageInitializeModel(this IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.TryGetValue("PageContext", out object pageContextObject))
            {
                var pageContext = pageContextObject as MvcPageContext;
                return pageContext.GetClientContext();
            }
            else
                return null;
        }

        public static Task<IHtmlContent> PageContentAsync(this IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.ContainsKey("ModelContext"))
                throw new InvalidOperationException();

            var metadataManager = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IContentMetadataManager>();
            if (metadataManager == null)
                throw new InvalidOperationException();

            var contentExplorer = ContentExplorer.Create(metadataManager, htmlHelper.ViewData.Model);
            if (!contentExplorer.Metadata.SupportViews)
                throw new InvalidOperationException("Модель контента не поддерживает представления.");

            var contentContext = new MvcContentContext(contentExplorer);
            return contentContext.RenderAsync(htmlHelper.ViewContext);
        }

        public static Task<IHtmlContent> RenderContentAsync<TModel, TValue>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression)
            where TModel : class
            where TValue : class
        {
            var path = expression.Body.ToString();

            return RenderContentAsync(htmlHelper, path);
        }
        public static Task<IHtmlContent> RenderContentAsync(this IHtmlHelper htmlHelper, string contentPath)
        {
            if (string.IsNullOrEmpty(contentPath))
                throw new ArgumentNullException(nameof(contentPath));

            if (!htmlHelper.ViewData.TryGetValue("ModelContext", out object currentModelContextObject))
                throw new InvalidOperationException();
            var currentModelContext = currentModelContextObject as MvcContentContext;

            var contentExplorer = currentModelContext.Explorer.Navigate(contentPath);
            if (!contentExplorer.Metadata.SupportViews)
                throw new InvalidOperationException("Модель контента не поддерживает представления.");

            var modelContext = new MvcContentContext(contentExplorer);
            return modelContext.RenderAsync(htmlHelper.ViewContext);
        }
    }
}