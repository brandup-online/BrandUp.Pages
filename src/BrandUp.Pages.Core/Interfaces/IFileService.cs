using System;
using System.IO;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IFileService
    {
        Task<IPageFile> FindFileByIdAsync(Guid fileId);
        Task<IPageFile> UploadFileAsync(string fileName, Stream fileStream);
        Task<Stream> GetFileStreamAsync(Guid fileId);
    }

    public interface IPageFile
    {
        Guid Id { get; }
        string Name { get; }
        long Size { get; }
    }
}