using BrandUp.Website;
using BrandUp.Website.Pages;

namespace LandingWebSite.Pages
{
    public class PageEvents : IPageEvents
    {
        public Task PageClientBuildAsync(PageClientBuildContext context)
        {
            return Task.CompletedTask;
        }

        public Task PageClientNavigationAsync(PageClientNavidationContext context)
        {
            return Task.CompletedTask;
        }

        public Task PageRequestAsync(PageRequestContext context)
        {
            return Task.CompletedTask;
        }

        public async Task PageRenderAsync(PageRenderContext context)
        {
            context.TagName = "main";

            context.Attributes.Add("role", "main");

            context.Attributes
                .AddCssClass("content-width")
                .AddCssClass("app-content");

            await Task.CompletedTask;
        }
    }
}