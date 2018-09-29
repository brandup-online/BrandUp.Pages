using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.Rendering
{
    public class MvcPageRendererTests : IAsyncLifetime
    {
        private readonly ServiceProvider serviceProvider;

        public MvcPageRendererTests()
        {
            var services = new ServiceCollection();
            new TestStartup(null).ConfigureServices(services);

            serviceProvider = services.BuildServiceProvider();
        }

        public async Task InitializeAsync()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var pageCollectionService = scope.ServiceProvider.GetService<IPageCollectionService>();
                var pageService = scope.ServiceProvider.GetService<IPageService>();

                var pageCollection = await pageCollectionService.CreateCollectionAsync("Test", "TestPage", PageSortMode.FirstOld, null);

                var page = await pageService.CreatePageAsync(pageCollection);
                await pageService.PublishPageAsync(page, "test");

                await pageService.SetDefaultPageAsync(page);
            }
        }

        public Task DisposeAsync()
        {
            serviceProvider.Dispose();

            return Task.CompletedTask;
        }

        [Fact]
        public async Task RenderPage()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var pageRenderer = scope.ServiceProvider.GetService<IPageRenderer>();

                var pageContext = new PageContext(null, null, null);

                using (var stream = new MemoryStream())
                {
                    await pageRenderer.RenderPageAsync(pageContext, stream);
                }
            }
        }
    }
}