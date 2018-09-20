using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Mvc.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace BrandUp.Pages.Mvc
{
    public class WebSiteBuilderTests
    {
        private TestServer server;

        public IServiceProvider Services => server.Host.Services;

        public WebSiteBuilderTests()
        {
            server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
        }

        [Fact]
        public void Resolve_ContentMetadataManager()
        {
            var service = Services.GetService<IContentMetadataManager>();

            Assert.NotNull(service);
        }

        [Fact]
        public void Resolve_ContentViewManager()
        {
            var service = Services.GetService<IContentViewManager>();

            Assert.NotNull(service);
        }

        [Fact]
        public void Resolve_PageTypeManager()
        {
            var service = Services.GetService<IPageMetadataManager>();

            Assert.NotNull(service);
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddApplicationPart(typeof(TestPageContent).Assembly)
                .AddWebSite();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        }
    }
}