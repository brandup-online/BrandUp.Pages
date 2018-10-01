using BrandUp.Pages.Content;
using BrandUp.Pages.Metadata;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
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
            server = new TestServer(new WebHostBuilder().UseStartup<TestStartup>());
        }

        [Fact]
        public void Resolve_ContentMetadataManager()
        {
            var service = Services.GetService<IContentMetadataManager>();

            Assert.NotNull(service);
            Assert.NotEmpty(service.GetAllMetadata());
        }

        [Fact]
        public void Resolve_PageMetadataManager()
        {
            var service = Services.GetService<IPageMetadataManager>();

            Assert.NotNull(service);
            Assert.NotEmpty(service.GetAllMetadata());
        }
    }
}