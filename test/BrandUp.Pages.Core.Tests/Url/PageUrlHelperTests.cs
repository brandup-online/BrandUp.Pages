using BrandUp.Pages.Builder;
using BrandUp.Pages.ContentModels;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Url
{
	public class PageUrlHelperTests : IAsyncLifetime
	{
		private ServiceProvider serviceProvider;
		private IServiceScope serviceScope;
		private readonly IPageUrlHelper pageUrlHelper;

		public PageUrlHelperTests()
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

			pageUrlHelper = serviceScope.ServiceProvider.GetService<IPageUrlHelper>();
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

		[Fact]
		public void GetDefaultPagePath()
		{
			var defaultPagePath = pageUrlHelper.GetDefaultPagePath();

			Assert.Equal("home", defaultPagePath);
		}

		[Theory]
		[InlineData("Test", "test")]
		[InlineData("/Test", "test")]
		[InlineData("/Test ", "test")]
		[InlineData("Test-", "test")]
		[InlineData("Test_", "test")]
		[InlineData("/Test/test", "test/test")]
		public void NormalizeUrlPath(string urlPath, string validValue)
		{
			var normalizedUrlPath = pageUrlHelper.NormalizeUrlPath(urlPath);

			Assert.Equal(validValue, normalizedUrlPath);
		}

		[Theory]
		[InlineData("Test", true)]
		[InlineData("Test_test", true)]
		[InlineData("Test-test", true)]
		[InlineData("/Test", false)]
		[InlineData("/Test ", false)]
		[InlineData("Test?test", false)]
		[InlineData("Test?test=test", false)]
		[InlineData("", false)]
		public void ValidateUrlPath(string urlPath, bool validValue)
		{
			var validateResult = pageUrlHelper.ValidateUrlPath(urlPath);

			Assert.Equal(validValue, validateResult.IsSuccess);
		}

		[Theory]
		[InlineData("home", true)]
		[InlineData("Home", true)]
		[InlineData("test", false)]
		[InlineData("test/home", false)]
		[InlineData("home/test", false)]
		public void IsDefaultUrlPath(string urlPath, bool validValue)
		{
			var result = pageUrlHelper.IsDefaultUrlPath(urlPath);

			Assert.Equal(validValue, result);
		}

		[Theory]
		[InlineData("home", "test", "home/test")]
		[InlineData("", "home", "home")]
		[InlineData("/test1/test2", "home", "test1/test2/home")]
		public void ExtendUrlPath(string urlPath, string urlPathName, string validResult)
		{
			var result = pageUrlHelper.ExtendUrlPath(urlPath, urlPathName);

			Assert.Equal(validResult, result);
		}

		#endregion
	}
}