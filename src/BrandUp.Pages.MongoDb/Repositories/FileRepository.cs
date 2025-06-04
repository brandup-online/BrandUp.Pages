using BrandUp.Pages.Files;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class FileRepository : IFileRepository
    {
        readonly FileBucket files;

        public FileBucket Files => files;

        public FileRepository(IPagesDbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            files = new FileBucket(dbContext.Database, new GridFSBucketOptions { BucketName = "BrandUpPages" });
        }

        #region IFileRepository members

        public async Task<IFile> UploadFileAsync(string contentKey, string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            var fileDoc = new FileDocument(contentKey, fileName, contentType);

            var uploadOptions = new GridFSUploadOptions
            {
                Metadata = MongoDbHelper.DictionaryToBsonDocument(fileDoc.Data)
            };

            await files.UploadFromStreamAsync(fileDoc.Id, fileName, stream, uploadOptions, cancellationToken);

            return fileDoc;
        }

        public async Task<IFile> FindFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<GridFSFileInfo<Guid>>.Filter.Eq(info => info.Id, fileId);
            var cursor = await files.FindAsync(filter, cancellationToken: cancellationToken);

            var fileInfo = await cursor.SingleOrDefaultAsync(cancellationToken);
            if (fileInfo == null)
                return null;

            return new FileDocument(fileInfo);
        }

        public async Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return await files.OpenDownloadStreamAsync(fileId, cancellationToken: cancellationToken);
        }

        public Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return files.DeleteAsync(fileId, cancellationToken);
        }

        #endregion

        public class FileBucket(IMongoDatabase database, GridFSBucketOptions options = null) : GridFSBucket<Guid>(database, options) { }
    }
}