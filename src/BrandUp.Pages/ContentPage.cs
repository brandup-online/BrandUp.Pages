using Microsoft.AspNetCore.Mvc.Razor;

namespace BrandUp.Pages
{
    public abstract class ContentPage<TModel> : RazorPage<TModel>
    {
        public ContentContext Content => ViewData[Views.ViewRenderService.ViewData_ContentContextKeyName] as ContentContext;
    }
}