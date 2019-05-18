using System;

namespace BrandUp.Pages.Content.Files
{
    public interface IFile
    {
        Guid Id { get; }
        string ContentType { get; }
        string EntryId { get; }
    }
}