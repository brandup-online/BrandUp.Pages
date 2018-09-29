using BrandUp.Pages.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Mvc
{
    public class TestStartup
    {
        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddPages()
                .AddContentTypesFromAssemblies(typeof(Helpers.TestPageContent).Assembly)
                .AddFakeRepositories()
                .UseMvcViews();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UsePages();
        }
    }
}