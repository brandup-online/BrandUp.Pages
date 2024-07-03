namespace BrandUp.Pages.Files
{
    public class FileService(IFileRepository repository)
    {
        public Task<IFile> UploadFileAsync(string contentKey, string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contentKey);
            ArgumentNullException.ThrowIfNull(fileName);
            ArgumentNullException.ThrowIfNull(contentType);
            ArgumentNullException.ThrowIfNull(stream);

            return repository.UploadFileAsync(contentKey, fileName, contentType, stream, cancellationToken);
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

        public string GetFileExtension(IFile file)
        {
            ArgumentNullException.ThrowIfNull(file);

            return Path.GetExtension(file.Name);
        }
    }
}