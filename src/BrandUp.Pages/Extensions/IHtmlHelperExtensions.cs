using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public static class IHtmlHelperExtensions
    {
        public static Task<IHtmlContent> RenderPageAsync<TPageModel>(this IHtmlHelper<TPageModel> htmlHelper)
            where TPageModel : ContentPageModel
        {
            var pageModel = htmlHelper.ViewData.Model;

            return htmlHelper.PartialAsync("~/Contents/Page/Default.cshtml", pageModel.PageContent);
        }
    }
}