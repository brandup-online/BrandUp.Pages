namespace BrandUp.Pages.Images
{
    public interface IImageResizer
    {
        Task ResizeAsync(Stream imageStream, int width, int height, Stream output, CancellationToken cancellationToken);
    }
}