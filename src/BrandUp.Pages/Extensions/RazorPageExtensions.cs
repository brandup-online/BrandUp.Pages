using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;

namespace BrandUp.Pages
{
    public static class RazorPageExtensions
    {
        public static IHtmlContent RenderPage(this RazorPage razorPage)
        {
            var t = typeof(RazorPage);
            var m = t.GetMethod("RenderBody", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            return (IHtmlContent)m.Invoke(razorPage, null);
        }
    }
}
