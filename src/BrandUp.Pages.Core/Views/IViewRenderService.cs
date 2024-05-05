namespace BrandUp.Pages.Views
{
	public interface IViewRenderService
	{
		Task RenderAsync(ContentContext contentContext, TextWriter output);
	}
}