using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    public static class IHtmlHelperExtensions
    {
        public static async Task<IHtmlContent> RenderPageContentAsync(this IHtmlHelper htmlHelper)
        {
            var viewRenderService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IViewRenderService>();
            var pageModel = htmlHelper.ViewData.Model;
            if (pageModel is not ContentPageModel contentPageModel)
                throw new InvalidOperationException($"Модель страницы должна наследовать {typeof(ContentPageModel).FullName}.");

            var builder = new HtmlContentBuilder();
            var pageHtml = await viewRenderService.RenderToStringAsync(contentPageModel.ContentContext);
            builder.AppendHtml(pageHtml);
            return builder;
        }
    }
}