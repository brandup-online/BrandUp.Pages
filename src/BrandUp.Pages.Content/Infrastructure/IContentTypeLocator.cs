using System.Reflection;

namespace BrandUp.Pages.Content.Infrastructure
{
	public interface IContentTypeLocator
	{
		IEnumerable<TypeInfo> ContentTypes { get; }
	}
}