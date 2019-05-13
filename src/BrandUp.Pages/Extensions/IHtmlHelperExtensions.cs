using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public static class IHtmlHelperExtensions
    {
        public static async Task<IHtmlContent> RenderPageAsync(this IHtmlHelper<ContentPageModel> htmlHelper)
        {
            var viewLocator = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<Views.IViewLocator>();
            var pageModel = htmlHelper.ViewData.Model;

            var view = viewLocator.FindView(pageModel.ContentContext.Explorer.Metadata.ModelType);
            if (view == null)
                throw new System.Exception();

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = pageModel.ContentContext.Content
            };
            viewData.Add("_ContentContext_", pageModel.ContentContext);

            var itemRenderingContext = new TagHelpers.ContentRenderingContext();
            viewData.Add("_ContentRenderingContext_", itemRenderingContext);

            var pageHtml = await htmlHelper.PartialAsync("~" + view.Name, pageModel.ContentContext.Content, viewData);

            string tagName = "div";

            if (!string.IsNullOrEmpty(itemRenderingContext.HtmlTag))
                tagName = itemRenderingContext.HtmlTag;

            var tag = new TagBuilder(tagName);
            if (!string.IsNullOrEmpty(itemRenderingContext.CssClass))
                tag.AddCssClass(itemRenderingContext.CssClass);

            tag.Attributes.Add("content-page", "");
            tag.Attributes.Add("content-path", pageModel.ContentContext.Explorer.Path);

            tag.InnerHtml.AppendHtml(pageHtml);

            return tag;
        }
    }
}