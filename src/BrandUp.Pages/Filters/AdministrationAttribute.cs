using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AdministrationAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var accessProvider = context.HttpContext.RequestServices.GetRequiredService<Identity.IAccessProvider>();
            if (!await accessProvider.CheckAccessAsync(context.HttpContext.RequestAborted))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}