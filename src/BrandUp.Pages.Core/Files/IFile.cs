using System;

namespace BrandUp.Pages.Files
{
    public interface IFile
    {
        Guid Id { get; }
        string ContentType { get; }
        string EntryId { get; }
    }
}