using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Content.Builder
{
	public interface IContentBuilder
	{
		IServiceCollection Services { get; }
	}

	public class ContentBuilder : IContentBuilder
	{
		public ContentBuilder(IServiceCollection services)
		{
			Services = services ?? throw new ArgumentNullException(nameof(services));

			AddServices(services);
		}

		public IServiceCollection Services { get; }

		private static void AddServices(IServiceCollection services)
		{
			services.AddSingleton<IContentMetadataManager, ContentMetadataManager>();
		}
	}
}