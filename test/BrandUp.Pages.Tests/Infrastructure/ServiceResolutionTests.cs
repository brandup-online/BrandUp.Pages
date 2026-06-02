using BrandUp.Pages.Builder;
using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using BrandUp.Pages.Tests.Infrastructure;
using BrandUp.Pages.Url;
using BrandUp.Pages.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Tests.Infrastructure
{
	/// <summary>
	/// Smoke test that the BrandUp.Pages services (consumed by controllers and tag-helpers)
	/// are correctly wired in the real application host.
	/// </summary>
	public class ServiceResolutionTests
	{
		[Theory]
		[InlineData(typeof(IPageService))]
		[InlineData(typeof(IPageCollectionService))]
		[InlineData(typeof(IPageContentService))]
		[InlineData(typeof(IPageMetadataManager))]
		[InlineData(typeof(IContentMetadataManager))]
		[InlineData(typeof(IViewLocator))]
		[InlineData(typeof(IPageLinkGenerator))]
		[InlineData(typeof(Identity.IAccessProvider))]
		public void Service_IsResolvable(Type serviceType)
		{
			using var factory = new PagesWebApplicationFactory();
			using var scope = factory.Services.CreateScope();

			var service = scope.ServiceProvider.GetService(serviceType);

			Assert.NotNull(service);
		}

		[Fact]
		public void ContentTypes_FromLandingSite_AreRegistered()
		{
			using var factory = new PagesWebApplicationFactory();
			using var scope = factory.Services.CreateScope();

			var metadataManager = scope.ServiceProvider.GetRequiredService<IPageMetadataManager>();

			Assert.NotEmpty(metadataManager.MetadataProviders);
		}
	}
}
