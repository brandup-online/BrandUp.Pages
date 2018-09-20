using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages
{
    public interface IWebSiteBuilder
    {
        IServiceCollection Services { get; }
    }

    public class WebSiteBuilder : IWebSiteBuilder
    {
        public WebSiteBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }

    public class WebSiteOptions
    {

    }
}