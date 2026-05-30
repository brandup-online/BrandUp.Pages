using System.Reflection;

namespace BrandUp.Pages.Content.Infrastructure
{
	public class AssemblyContentTypeLocator : IContentTypeLocator
	{
		readonly HashSet<TypeInfo> types = [];

		public AssemblyContentTypeLocator(Assembly[] assemblies)
		{
			if (assemblies == null)
				throw new ArgumentNullException(nameof(assemblies));

			foreach (var assembly in assemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					var typeInfo = type.GetTypeInfo();

					if (!ContentMetadataManager.TypeIsContent(typeInfo))
						continue;

					types.Add(typeInfo);
				}
			}
		}

		public IEnumerable<TypeInfo> ContentTypes => types;
	}
}