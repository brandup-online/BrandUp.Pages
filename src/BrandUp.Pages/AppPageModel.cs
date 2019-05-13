using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public abstract class AppPageModel : PageModel, IContentPageModel
    {
        private bool rendenOnlyContent = false;
        private readonly IDictionary<string, object> clientModel = new Dictionary<string, object>();

        #region IContentPageModel members

        public abstract string Title { get; }
        public virtual string Description => null;
        public virtual string Keywords => null;
        public virtual string CssClass => null;
        public virtual string ScriptName => null;

        #endregion

        #region Handler methods

        public async Task<IActionResult> OnGetNavigationAsync()
        {
            var navModel = await GetNavigationModelAsync();

            return new OkObjectResult(navModel);
        }
        public IActionResult OnGetContent()
        {
            rendenOnlyContent = true;

            return Page();
        }

        #endregion

        public void RenderPage(Microsoft.AspNetCore.Mvc.Razor.IRazorPage page)
        {
            if (rendenOnlyContent)
                page.Layout = null;
        }
        internal async Task<Models.NavigationClientModel> GetNavigationModelAsync()
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;
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
                Page = await GetClientModelAsync()
            };

            navModel.Query.Remove("handler");

            var antiforgery = httpContext.RequestServices.GetRequiredService<IAntiforgery>();
            if (antiforgery != null)
            {
                var antiforgeryTokenSet = antiforgery.GetTokens(httpContext);
                navModel.ValidationToken = antiforgeryTokenSet.RequestToken;
            }

            return navModel;
        }
        internal async Task<Models.PageClientModel> GetClientModelAsync()
        {
            var model = new Models.PageClientModel
            {
                Title = Title,
                CssClass = CssClass,
                ScriptName = ScriptName,
                Data = new Dictionary<string, object>()
            };

            var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);
            foreach (var p in properties)
            {
                var attr = p.GetCustomAttribute<ClientModelAttribute>();
                if (attr == null)
                    continue;

                var name = p.Name;
                if (!string.IsNullOrEmpty(attr.Name))
                    name = attr.Name;

                name = name.Substring(0, 1).ToLower() + name.Substring(1);

                var value = p.GetValue(this);
                if (value is Enum)
                    value = value.ToString();

                model.Data.Add(name, value);
            }

            await OnBuildClientData(model.Data);

            return model;
        }

        protected virtual Task OnBuildClientData(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }
    }
}