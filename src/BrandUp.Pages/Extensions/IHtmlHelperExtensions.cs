using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public static class IHtmlHelperExtensions
    {
        public static async Task<IHtmlContent> RenderPageAsync(this IHtmlHelper<ContentPageModel> htmlHelper)
        {
            var viewRenderService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<Views.IViewRenderService>();
            var pageModel = htmlHelper.ViewData.Model;

            var pageHtml = await viewRenderService.RenderToStringAsync(pageModel.ContentContext);

            var builder = new HtmlContentBuilder();
            builder.AppendHtml(pageHtml);
            return builder;
        }
    }
}