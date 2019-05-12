using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public static class IHtmlHelperExtensions
    {
        public static async Task<IHtmlContent> RenderPageAsync<TPageModel>(this IHtmlHelper<TPageModel> htmlHelper)
            where TPageModel : ContentPageModel
        {
            var pageModel = htmlHelper.ViewData.Model;

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = pageModel.ContentContext.Content
            };

            viewData.Add("_ContentContext_", pageModel.ContentContext);

            var tag = new TagBuilder("div");

            tag.InnerHtml.AppendHtml(await htmlHelper.PartialAsync("~/Contents/Page/Article.cshtml", pageModel.ContentContext.Content, viewData));

            return tag;
        }
    }
}