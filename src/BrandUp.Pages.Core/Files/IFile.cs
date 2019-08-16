using System;

namespace BrandUp.Pages.Files
{
    public interface IFile
    {
        Guid Id { get; }
        string ContentType { get; }
        string Name { get; }
        Guid PageId { get; }
    }
}