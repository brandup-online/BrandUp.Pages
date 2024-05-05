namespace BrandUp.Pages.Images
{
	public interface IImageResizer
	{
		Task Resize(Stream imageStream, int width, int height, Stream output);
	}
}