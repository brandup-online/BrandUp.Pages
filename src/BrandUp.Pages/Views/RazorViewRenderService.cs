using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace BrandUp.Pages.Views
{
    public class RazorViewRenderService(ICompositeViewEngine viewEngine, IHttpContextAccessor httpContextAccessor, IViewLocator viewLocator, HtmlEncoder htmlEncoder) : IViewRenderService
    {
        public const string ViewData_ContentContextKeyName = "_ContentContext_";
        public const string ViewData_ViewRenderingContextKeyName = "_ViewRenderingContext_";

        readonly ICompositeViewEngine viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        readonly IViewLocator viewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));
        readonly HtmlEncoder htmlEncoder = htmlEncoder ?? throw new ArgumentNullException(nameof(htmlEncoder));

        #region IViewRenderService members

        public async Task RenderAsync(ContentContext contentContext, TextWriter output)
        {
            ArgumentNullException.ThrowIfNull(contentContext);
            ArgumentNullException.ThrowIfNull(output);

            var contentView = viewLocator.FindView(contentContext.Explorer.Metadata.ModelType);
            if (contentView == null)
                throw new InvalidOperationException($"Couldn't find content view {contentContext.Explorer.Metadata.Name}");

            var viewEngineResult = viewEngine.GetView("~/", contentView.Name, false);
            if (!viewEngineResult.Success)
                throw new InvalidOperationException($"Couldn't find view {contentView.Name}");
            var view = viewEngineResult.View;

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = contentContext.Content };
            viewData.Add(ViewData_ContentContextKeyName, contentContext);

            var itemRenderingContext = new ViewRenderingContext();
            viewData.Add(ViewData_ViewRenderingContextKeyName, itemRenderingContext);

            using var contentOutput = new StringWriter();

            var viewContext = new ViewContext
            {
                HttpContext = httpContextAccessor.HttpContext,
                ViewData = viewData,
                Writer = contentOutput,
                RouteData = new RouteData()
            };

            await view.RenderAsync(viewContext);

            string tagName = "div";
            if (!string.IsNullOrEmpty(itemRenderingContext.HtmlTag))
                tagName = itemRenderingContext.HtmlTag;

            var tag = new TagBuilder(tagName);
            if (!string.IsNullOrEmpty(itemRenderingContext.CssClass))
                tag.AddCssClass(itemRenderingContext.CssClass);

            if (!string.IsNullOrEmpty(itemRenderingContext.ScriptName))
                tag.Attributes.Add("data-content-script", itemRenderingContext.ScriptName);

            if (contentContext.Explorer.IsRoot)
            {
                tag.Attributes.Add("data-content-root", contentContext.Key);
                if (contentContext.IsDesigner)
                    tag.Attributes.Add("data-content-edit-id", contentContext.EditId.Value.ToString());
            }
            tag.Attributes.Add("data-content-type", contentContext.Explorer.Metadata.Name);
            tag.Attributes.Add("data-content-title", contentContext.Explorer.Metadata.Title);
            tag.Attributes.Add("data-content-path", contentContext.Explorer.ModelPath);
            //tag.Attributes.Add("content-path-index", contentContext.Explorer.Index.ToString());

            tag.InnerHtml.AppendHtml(contentOutput.ToString());

            tag.WriteTo(output, htmlEncoder);
        }

        #endregion
    }
}