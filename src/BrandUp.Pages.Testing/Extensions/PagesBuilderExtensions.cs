using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Builder
{
	public static class PagesBuilderExtensions
	{
		public static IPagesBuilder AddFakes(this IPagesBuilder builder)
		{
			builder.Services.AddPagesTesting();

			builder
				.AddUserAccessProvider<Identity.FakeAccessProvider>(ServiceLifetime.Scoped);

			return builder;
		}
	}
}