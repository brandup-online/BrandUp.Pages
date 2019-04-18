using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Content
{
    public class FileService
    {
        private readonly IFileRepository repository;

        public FileService(IFileRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<FileEntry> UploadFileAsync(string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            var fileDocument = await repository.UploadFileAsync(fileName, contentType, stream, cancellationToken);

            return new FileEntry(fileDocument);
        }
        public async Task<FileEntry> FindFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            if (fileId == Guid.Empty)
                throw new ArgumentException();

            var fileDocument = await repository.FindFileByIdAsync(fileId, cancellationToken);
            if (fileDocument == null)
                return null;

            return new FileEntry(fileDocument);
        }
        public Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            if (fileId == Guid.Empty)
                throw new ArgumentException();

            return repository.ReadFileAsync(fileId, cancellationToken);
        }
    }

    public interface IFileRepository
    {
        Task<IFileDocument> UploadFileAsync(string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default);
        Task<IFileDocument> FindFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default);
        Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    }

    public class FileEntry
    {
        public Guid Id { get; }
        public string ContentType { get; }

        public FileEntry(IFileDocument fileDocument)
        {
            if (fileDocument == null)
                throw new ArgumentNullException(nameof(fileDocument));

            Id = fileDocument.Id;
            ContentType = fileDocument.ContentType;
        }
    }

    public interface IFileDocument
    {
        Guid Id { get; }
        string ContentType { get; }
    }
}