using BrandUp.Pages.Content;

namespace BrandUp.Pages
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class PageContentAttribute : ContentTypeAttribute
	{
	}
}