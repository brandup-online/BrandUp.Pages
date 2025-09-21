using BrandUp.Pages.Identity;
using BrandUp.Website;
using BrandUp.Website.Pages;

namespace LandingWebSite.Pages
{
    public class PageEvents(IAccessProvider pageAccessProvider) : IPageEvents
    {
        readonly IAccessProvider pageAccessProvider = pageAccessProvider ?? throw new ArgumentNullException(nameof(pageAccessProvider));

        public async Task PageClientBuildAsync(PageClientBuildContext context)
        {
            var enableAdmin = await pageAccessProvider.CheckAccessAsync(context.CancellationToken);
            context.ClientData.Add("enableAdministration", enableAdmin);
        }

        public async Task PageClientNavigationAsync(PageClientNavidationContext context)
        {
            await Task.CompletedTask;
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

        public Task PageRequestAsync(PageRequestContext context)
        {
            return Task.CompletedTask;
        }
    }
}