using BrandUp.Pages.Content;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BrandUp.Pages.Mvc
{
    public class MvcContentContext
    {
        public ContentExplorer Explorer { get; }

        public MvcContentContext(ContentExplorer explorer)
        {
            Explorer = explorer ?? throw new ArgumentNullException(nameof(explorer));
        }

        public async Task<IHtmlContent> RenderAsync(ViewContext viewContext)
        {
            var view = Explorer.ViewManager.GetContentView(Explorer.Content);

            var model = Explorer.Content;
            var viewData = new ViewDataDictionary<object>(viewContext.ViewData.ModelMetadata.GetMetadataForType(model.GetType()), new ModelStateDictionary()) { Model = model };
            viewData.Add("ModelContext", this);

            using (var writer = new StringWriter())
            {
                var newViewContext = new ViewContext(viewContext, viewContext.View, viewData, viewContext.TempData, writer, new HtmlHelperOptions());

                var temp = view.Name.Split(new char[] { '.' });
                var viewName = "~/ContentViews/" + temp[0] + "/" + temp[1] + ".cshtml";

                await RenderPartialViewAsync(newViewContext, viewName);

                var modelTag = new TagBuilder("div") { TagRenderMode = TagRenderMode.Normal };
                modelTag.MergeAttribute("data-content-model", Explorer.Metadata.Name);
                modelTag.MergeAttribute("data-content-path", Explorer.Path);
                if (Explorer.Index >= 0)
                    modelTag.MergeAttribute("data-content-index", Explorer.Index.ToString());
                //modelTag.MergeAttribute("data-content-view", viewName);
                modelTag.InnerHtml.SetHtmlContent(new HtmlString(writer.ToString()));
                return modelTag;
            }
        }

        private async Task<string> RenderPartialViewAsync(ViewContext context, string viewName)
        {
            var viewEngine = context.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

            var viewResult = viewEngine.GetView(context.ExecutingFilePath, viewName, false);
            if (!viewResult.Success)
                viewResult = viewEngine.FindView(context, viewName, false);
            if (!viewResult.Success)
                throw new InvalidOperationException($"Не найдено представление {viewName}.");

            await viewResult.View.RenderAsync(context);

            return context.Writer.ToString();

        }
    }
}