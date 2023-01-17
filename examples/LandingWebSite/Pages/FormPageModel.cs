using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace LandingWebSite.Pages
{
    public abstract class FormPageModel : AppPageModel
    {
        public const string ReturnUrlParamName = "returnurl";

        public string ReturnUrl { get; private set; }

        public override string ScriptName => "form";
        protected override async Task OnPageRequestAsync(PageRequestContext context)
        {
            if (Request.Query.TryGetValue(ReturnUrlParamName, out StringValues returnUrlValue))
                ReturnUrl = returnUrlValue[0];
            else
                ReturnUrl = DefaultReturnUrl;

            if (string.IsNullOrEmpty(ReturnUrl))
                ReturnUrl = Url.Page("/Index", new { area = "" });

            await base.OnPageRequestAsync(context);
        }

        protected abstract string DefaultReturnUrl { get; }
    }
}