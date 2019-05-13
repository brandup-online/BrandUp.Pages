using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages
{
    public abstract class AppPageModel : PageModel, IContentPageModel
    {
        private bool rendenOnlyContent = false;

        #region IContentPageModel members

        public abstract string Title { get; }
        public virtual string Description => null;
        public virtual string Keywords => null;

        #endregion

        public IActionResult OnGetNavigation([FromServices]IAntiforgery antiforgery)
        {
            var navModel = GetNavigationModel();

            return new OkObjectResult(navModel);
        }

        public IActionResult OnGetContent()
        {
            rendenOnlyContent = true;

            return Page();
        }

        public void RenderPage(Microsoft.AspNetCore.Mvc.Razor.IRazorPage page)
        {
            if (rendenOnlyContent)
                page.Layout = null;
        }
        internal Models.NavigationClientModel GetNavigationModel()
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;
            var antiforgery = httpContext.RequestServices.GetRequiredService<IAntiforgery>();
            var antiforgeryTokenSet = antiforgery.GetTokens(httpContext);
            var requestUrl = httpRequest.GetDisplayUrl();

            if (httpRequest.Query.ContainsKey("handler"))
            {
                var uri = new Uri(requestUrl);
                var baseUri = uri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);

                var query = QueryHelpers.ParseQuery(uri.Query);
                query.Remove("handler");
                var qb = new QueryBuilder();
                foreach (var kv in query)
                    qb.Add(kv.Key, (IEnumerable<string>)kv.Value);

                requestUrl = baseUri + qb.ToQueryString();
            }

            var navModel = new Models.NavigationClientModel
            {
                IsAuthenticated = httpContext.User.Identity.IsAuthenticated,
                Url = requestUrl,
                Query = new Dictionary<string, StringValues>(httpRequest.Query),
                ValidationToken = antiforgeryTokenSet.RequestToken,
                Page = new Models.PageClientModel
                {
                    Title = Title,
                    CssClass = null,
                    ScriptName = null
                }
            };

            return navModel;
        }
    }
}