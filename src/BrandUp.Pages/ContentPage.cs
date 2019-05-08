using Microsoft.AspNetCore.Mvc.Razor;

namespace BrandUp.Pages
{
    public abstract class ContentPage<TModel> : RazorPage<TModel>
    {
        public ContentContext Content => ViewData["_ContentContext_"] as ContentContext;
    }
}