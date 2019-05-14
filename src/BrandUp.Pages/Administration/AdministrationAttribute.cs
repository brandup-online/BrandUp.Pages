using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Administration
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AdministrationAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var administrationManager = context.HttpContext.RequestServices.GetRequiredService<IAdministrationManager>();
            if (!await administrationManager.CheckAsync())
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}