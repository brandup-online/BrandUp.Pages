using BrandUp.Pages.Builder;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.Url
{
    public class PageUrlPathGeneratorTests : IAsyncLifetime
    {
        private ServiceProvider serviceProvider;
        private IServiceScope serviceScope;
        private readonly IPageUrlPathGenerator pageUrlPathGenerator;

        public PageUrlPathGeneratorTests()
        {
            var services = new ServiceCollection();

            services.AddPages(options =>
            {
                options.DefaultPagePath = "Home";
            })
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakes();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageUrlPathGenerator = serviceScope.ServiceProvider.GetService<IPageUrlPathGenerator>();
        }

        #region IAsyncLifetime members

        Task IAsyncLifetime.InitializeAsync()
        {
            return Task.CompletedTask;
        }
        Task IAsyncLifetime.DisposeAsync()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();

            return Task.CompletedTask;
        }

        #endregion

        #region Test methods

        [Theory]
        [InlineData("1привет", "1privet")]
        [InlineData("привет1", "privet1")]
        [InlineData("привет привет", "privet-privet")]
        [InlineData("привет_привет", "privet-privet")]
        [InlineData("привет  привет", "privet-privet")]
        [InlineData("привет__привет", "privet-privet")]
        [InlineData("Привет", "privet")]
        [InlineData("ПРИВЕТ", "privet")]
        [InlineData("ПРИВЕТ-", "privet")]
        [InlineData("ПРИВЕТ_", "privet")]
        [InlineData("ПРИВЕТ%", "privet")]
        [InlineData(@"привет%!@#$%^&*()_+=-~`\?/", "privet")]
        public async Task Generate(string input, string result)
        {
            var urlPath = await pageUrlPathGenerator.GenerateAsync(new Page(input));

            Assert.Equal(result, urlPath);
        }

        #endregion

        private class Page : IPage
        {
            public Guid Id => throw new NotImplementedException();
            public DateTime CreatedDate => throw new NotImplementedException();
            public string WebSiteId => throw new NotImplementedException();
            public string TypeName => throw new NotImplementedException();
            public Guid OwnCollectionId => throw new NotImplementedException();
            public string Header { get; set; }
            public string UrlPath { get; set; }
            public string EntryId => throw new NotImplementedException();

            public bool IsPublished => throw new NotImplementedException();

            public Page(string header)
            {
                Header = header;
            }

            public Task SetUrlAsync(string urlPath)
            {
                throw new NotImplementedException();
            }
        }
    }
}