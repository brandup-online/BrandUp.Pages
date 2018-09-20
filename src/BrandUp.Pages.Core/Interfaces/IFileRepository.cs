using System;
using System.IO;
using System.Threading.Tasks;

namespace BrandUp.Pages.Interfaces
{
    public interface IFileRepository
    {
        Task<IPageFile> FindFileByIdAsync(Guid fileId);
        Task<Guid> UploadFileAsync(string fileName, Stream fileStream);
        Task<Stream> GetFileStreamAsync(Guid fileId);
    }
}