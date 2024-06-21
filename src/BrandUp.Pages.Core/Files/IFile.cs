namespace BrandUp.Pages.Files
{
    public interface IFile
    {
        Guid Id { get; }
        string WebsiteId { get; }
        string ContentKey { get; }
        string ContentType { get; }
        string Name { get; }
    }
}