using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace BrandUp.Pages.Tests
{
	public class UnitTest1 : IClassFixture<CustomWebApplicationFactory>
	{
		private readonly CustomWebApplicationFactory factory;

		public UnitTest1(CustomWebApplicationFactory factory)
		{
			this.factory = factory;
		}

		//[Fact]
		//public async Task Test1()
		//{
		//    using (var client = factory.CreateClient())
		//    {
		//        var response = await client.GetAsync("/");

		//        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		//    }
		//}

		//[Fact]
		//public void Test2()
		//{
		//    using (var client = factory.CreateClient())
		//    {
		//        using (var scope = factory.Server.Host.Services.CreateScope())
		//        {
		//            var viewLocator = scope.ServiceProvider.GetRequiredService<Views.IViewLocator>();
		//        }
		//    }
		//}
	}

	public class CustomWebApplicationFactory : WebApplicationFactory<LandingWebSite.Startup>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureTestServices(services =>
			{
			});
		}
	}
}