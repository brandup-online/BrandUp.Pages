using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Files
{
    public interface IFileRepository
    {
        Task<IFile> UploadFileAsync(string entryId, string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default);
        Task<IFile> FindFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default);
        Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    }
}