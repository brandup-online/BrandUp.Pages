using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BrandUp.Pages.TagHelpers
{
    [HtmlTargetElement("page", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PageTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public override int Order => int.MaxValue;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is not PageModel)
                return;

            if (ViewContext.ViewData[RazorViewRenderService.ViewData_ContentContextKeyName] is not ContentContext contentContext)
                return;
            if (ViewContext.ViewData[RazorViewRenderService.ViewData_ContentPageContextKeyName] is not ContentPageContext)
                return;

            if (!contentContext.Explorer.IsRoot)
                throw new InvalidOperationException();

            output.Attributes.Add("data-content-root", contentContext.Key);
            if (contentContext.IsDesigner)
                output.Attributes.Add("data-content-edit-id", contentContext.EditId.Value.ToString());
            output.Attributes.Add("data-content-type", contentContext.Explorer.Metadata.Name);
            output.Attributes.Add("data-content-title", contentContext.Explorer.Metadata.Title);
            output.Attributes.Add("data-content-path", contentContext.Explorer.ModelPath);

            base.Process(context, output);
        }
    }
}