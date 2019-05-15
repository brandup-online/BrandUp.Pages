using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace BrandUp.Pages.Views
{
    public class ViewRenderService : IViewRenderService
    {
        public const string ViewData_ContentContextKeyName = "_ContentContext_";
        public const string ViewData_ViewRenderingContextKeyName = "_ViewRenderingContext_";

        private readonly ICompositeViewEngine viewEngine;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IViewLocator viewLocator;
        private readonly HtmlEncoder htmlEncoder;

        public ViewRenderService(ICompositeViewEngine viewEngine, IHttpContextAccessor httpContextAccessor, IViewLocator viewLocator, HtmlEncoder htmlEncoder)
        {
            this.viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.viewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));
            this.htmlEncoder = htmlEncoder ?? throw new ArgumentNullException(nameof(htmlEncoder));
        }

        #region IViewRenderService members

        public async Task RenderAsync(ContentContext contentContext, TextWriter output)
        {
            if (contentContext == null)
                throw new ArgumentNullException(nameof(contentContext));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var contentView = viewLocator.FindView(contentContext.Explorer.Metadata.ModelType);
            if (contentView == null)
                throw new InvalidOperationException($"Couldn't find content view {contentView.Name}");

            var viewEngineResult = viewEngine.GetView("~/", contentView.Name, false);
            if (!viewEngineResult.Success)
                throw new InvalidOperationException($"Couldn't find view {contentView.Name}");
            var view = viewEngineResult.View;

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = contentContext.Content
            };
            viewData.Add(ViewData_ContentContextKeyName, contentContext);

            var itemRenderingContext = new ViewRenderingContext();
            viewData.Add(ViewData_ViewRenderingContextKeyName, itemRenderingContext);

            using (var contentOutput = new StringWriter())
            {
                var http = contentContext.Services.GetRequiredService<IHttpContextAccessor>();

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

                if (contentContext.Explorer.IsRoot)
                    tag.Attributes.Add("content-root", string.Empty);
                tag.Attributes.Add("content-type", contentContext.Explorer.Metadata.Name);
                tag.Attributes.Add("content-path", contentContext.Explorer.Path);
                tag.Attributes.Add("content-path-index", contentContext.Explorer.Index.ToString());

                tag.InnerHtml.AppendHtml(contentOutput.ToString());

                tag.WriteTo(output, htmlEncoder);
            }
        }

        #endregion
    }
}