using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages
{
    public abstract class AppPageModel : PageModel, IPageModel
    {
        private bool renderOnlyContent = false;
        private IPageNavigationProvider pageNavigationProvider;

        #region IContentPageModel members

        public abstract string Title { get; }
        public virtual string Description => null;
        public virtual string Keywords => null;
        public virtual string CssClass => null;
        public virtual string ScriptName => null;
        public virtual string CanonicalLink => null;

        #endregion

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            await OnInitializeAsync(context);

            pageNavigationProvider = HttpContext.RequestServices.GetService<IPageNavigationProvider>();
            if (pageNavigationProvider != null)
                await pageNavigationProvider.InitializeAsync(HttpContext.RequestAborted);

            if (Request.Query.ContainsKey("_content"))
                renderOnlyContent = true;

            await base.OnPageHandlerExecutionAsync(context, next);
        }

        #region Handler methods

        public async Task<IActionResult> OnGetNavigationAsync()
        {
            var navModel = await GetNavigationClientModelAsync();

            return new OkObjectResult(navModel);
        }
        public IActionResult OnGetContent()
        {
            renderOnlyContent = true;

            return Page();
        }

        #endregion

        public void RenderPage(Microsoft.AspNetCore.Mvc.Razor.IRazorPage page)
        {
            if (renderOnlyContent)
                page.Layout = null;
        }
        internal async Task<Models.AppClientModel> GetAppClientModelAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;

            var appClientModel = new Models.AppClientModel
            {
                BaseUrl = httpRequest.PathBase.HasValue ? httpRequest.PathBase.Value : "/",
                Data = new Dictionary<string, object>()
            };

            var antiforgery = httpContext.RequestServices.GetService<IAntiforgery>();
            if (antiforgery != null)
            {
                var antiforgeryToken = antiforgery.GetAndStoreTokens(httpContext);

                appClientModel.Antiforgery = new Models.AntiforgeryModel
                {
                    HeaderName = antiforgeryToken.HeaderName,
                    FormFieldName = antiforgeryToken.FormFieldName
                };
            }

            appClientModel.Nav = await GetNavigationClientModelAsync(cancellationToken);

            await OnBuildApplicationClientDataAsync(appClientModel.Data, cancellationToken);

            if (pageNavigationProvider != null)
                await pageNavigationProvider.BuildApplicationClientDataAsync(appClientModel.Data, cancellationToken);

            return appClientModel;
        }
        private async Task<Models.NavigationClientModel> GetNavigationClientModelAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = HttpContext;
            var httpRequest = httpContext.Request;
            var requestUrl = httpRequest.GetDisplayUrl();
            var requestUri = new Uri(requestUrl);
            var baseUri = requestUri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);

            if (httpRequest.Query.ContainsKey("handler"))
            {
                var query = QueryHelpers.ParseQuery(requestUri.Query);
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
                Path = requestUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped),
                Query = new Dictionary<string, object>(),
                Data = new Dictionary<string, object>()
            };

            foreach (var kv in httpRequest.Query)
            {
                var value = kv.Value;
                if (value.Count == 1)
                    navModel.Query.Add(kv.Key, value[0]);
                else if (value.Count > 1)
                    navModel.Query.Add(kv.Key, value.ToArray());
            }

            navModel.Query.Remove("handler");

            var accessProvider = httpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
            navModel.EnableAdministration = await accessProvider.CheckAccessAsync(cancellationToken);

            var antiforgery = httpContext.RequestServices.GetService<IAntiforgery>();
            if (antiforgery != null)
            {
                var antiforgeryTokenSet = antiforgery.GetTokens(httpContext);
                navModel.ValidationToken = antiforgeryTokenSet.RequestToken;
            }

            await OnBuildNavigationClientDataAsync(navModel.Data, cancellationToken);

            if (pageNavigationProvider != null)
                await pageNavigationProvider.BuildNavigationClientDataAsync(navModel.Data, cancellationToken);

            navModel.Page = await GetPageClientModelAsync(cancellationToken);

            return navModel;
        }
        private async Task<Models.PageClientModel> GetPageClientModelAsync(CancellationToken cancellationToken = default)
        {
            var model = new Models.PageClientModel
            {
                Title = Title,
                CssClass = CssClass,
                ScriptName = ScriptName,
                CanonicalLink = CanonicalLink,
                Description = Description,
                Keywords = Keywords,
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

            await OnBuildPageClientDataAsync(model.Data, cancellationToken);

            if (pageNavigationProvider != null)
                await pageNavigationProvider.BuildPageClientDataAsync(model.Data, cancellationToken);

            return model;
        }

        protected virtual Task OnInitializeAsync(PageHandlerExecutingContext context)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnBuildApplicationClientDataAsync(IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnBuildNavigationClientDataAsync(IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnBuildPageClientDataAsync(IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}