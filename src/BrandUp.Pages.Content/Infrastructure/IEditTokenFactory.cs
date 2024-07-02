namespace BrandUp.Pages.Content.Infrastructure
{
    public interface IEditTokenFactory
    {
        Task GenerateBeginEditTokenAsync(CancellationToken cancellationToken);
    }
}