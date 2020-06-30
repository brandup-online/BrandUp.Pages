using BrandUp.Pages.Identity;
using BrandUp.Website.Pages;
using System;
using System.Threading.Tasks;

namespace LandingWebSite.Pages
{
    public class PageEvents : IPageEvents
    {
        readonly IAccessProvider pageAccessProvider;

        public PageEvents(IAccessProvider pageAccessProvider)
        {
            this.pageAccessProvider = pageAccessProvider ?? throw new ArgumentNullException(nameof(pageAccessProvider));
        }

        public Task PageBuildAsync(PageBuildContext context)
        {
            return Task.CompletedTask;
        }

        public async Task PageNavigationAsync(PageNavidationContext context)
        {
            var enableAdmin = await pageAccessProvider.CheckAccessAsync(context.CancellationToken);
            context.ClientData.Add("enableAdministration", enableAdmin);
        }

        public Task PageRenderAsync(PageRenderContext context)
        {
            return Task.CompletedTask;
        }

        public Task PageRequestAsync(PageRequestContext context)
        {
            return Task.CompletedTask;
        }
    }
}