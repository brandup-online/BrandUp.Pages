using BrandUp.Pages.Content;
using BrandUp.Pages.Features;
using Microsoft.AspNetCore.Http;

namespace BrandUp.Pages
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Check content is editing in this http context.
        /// </summary>
        public static bool IsEditContent(this HttpContext httpContext, IContent content, out IContentEditContext editContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);
            ArgumentNullException.ThrowIfNull(content);

            var contentEditFeature = httpContext.Features.Get<ContentEditFeature>();
            if (contentEditFeature != null && contentEditFeature.IsCurrentEdit(content))
            {
                editContext = contentEditFeature;
                return true;
            }
            else
            {
                editContext = null;
                return false;
            }
        }
    }
}