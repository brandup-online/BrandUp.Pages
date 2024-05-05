using System.Reflection;

namespace BrandUp.Pages.Content.Infrastructure
{
	public class EmptyContentTypeLocator : IContentTypeLocator
	{
		public IEnumerable<TypeInfo> ContentTypes => Enumerable.Empty<TypeInfo>();
	}
}