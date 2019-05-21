using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Files
{
    public class FileService
    {
        private readonly IFileRepository repository;

        public FileService(IFileRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<IFile> UploadFileAsync(Content.IContentEntry contentEntry, string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            if (contentEntry == null)
                throw new ArgumentNullException(nameof(contentEntry));
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            return repository.UploadFileAsync(contentEntry.EntryId, fileName, contentType, stream, cancellationToken);
        }
        public Task<IFile> FindFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            if (fileId == Guid.Empty)
                throw new ArgumentException();

            return repository.FindFileByIdAsync(fileId, cancellationToken);
        }
        public Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            if (fileId == Guid.Empty)
                throw new ArgumentException();

            return repository.ReadFileAsync(fileId, cancellationToken);
        }
        public Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return repository.DeleteFileAsync(fileId, cancellationToken);
        }
    }
}