using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BrandUp.Pages.Middlewares
{
    public class PageMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestPath = context.Request.Path.Value;

            var pageService = context.RequestServices.GetRequiredService<IPageService>();

            var page = await pageService.FindPageByPathAsync(requestPath);
            if (page != null)
            {
                context.Response.StatusCode = 200;

                var pageMetadata = await pageService.GetPageTypeAsync(page);
                var pageContent = await pageService.GetPageContentAsync(page);

                var pageContext = new Rendering.PageContext(page, pageMetadata, pageContent);
                var pageRenderer = context.RequestServices.GetRequiredService<Rendering.IPageRenderer>();

                await pageRenderer.RenderPageAsync(pageContext, context.Response.Body);
            }
            else
                await next.Invoke(context);
        }
    }
}