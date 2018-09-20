using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace BrandUp.Pages.Content
{
    public class ContentViewManagerTests : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private readonly IContentViewManager manager;

        public ContentViewManagerTests()
        {
            var services = new ServiceCollection();

            services.AddWebSiteCore()
                .UseContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .UseContentViewsFromAttributes();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            manager = serviceScope.ServiceProvider.GetService<IContentViewManager>();
        }

        public void Dispose()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();
        }

        [Fact]
        public void FindViewByName()
        {
            var viewName = "TestPage.Default";
            var view = manager.FindViewByName(viewName);
            Assert.NotNull(view);
            Assert.Equal(view.Name, viewName);
        }

        [Fact]
        public void GetViews()
        {
            var views = manager.GetViews(typeof(TestPageContent));
            Assert.NotNull(views);
            Assert.True(views.Count() > 0);
        }

        [Fact]
        public void GetContentView()
        {
            var view = manager.GetContentView(new TestPageContent { ViewName = "TestPage.Default" });
            Assert.NotNull(view);
        }

        [Fact]
        public void GetContentView_Default()
        {
            var view = manager.GetContentView(new TestPageContent { ViewName = null });
            Assert.NotNull(view);
        }
    }
}