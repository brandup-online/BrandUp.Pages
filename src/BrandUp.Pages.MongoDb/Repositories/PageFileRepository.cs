using BrandUp.Pages.Files;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageFileRepository : IFileRepository
    {
        private readonly FileBucket files;

        public PageFileRepository(IPagesDbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            var options = new GridFSBucketOptions { BucketName = "BrandUpPages" };
            files = new FileBucket(dbContext.Database, options);
        }

        public async Task<IFile> UploadFileAsync(Guid pageId, string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            var fileDoc = new PageFileDocument(pageId, fileName, contentType);

            var uploadOptions = new GridFSUploadOptions
            {
                Metadata = MongoDbHelper.DictionaryToBsonDocument(fileDoc.Data)
            };

            await files.UploadFromStreamAsync(GuidConverter.ToBytes(fileDoc.Id, GuidRepresentation.Standard), fileName, stream, uploadOptions, cancellationToken);

            return fileDoc;
        }

        public async Task<IFile> FindFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<GridFSFileInfo<byte[]>>.Filter.Eq(info => info.Id, GuidConverter.ToBytes(fileId, GuidRepresentation.Standard));
            var cursor = await files.FindAsync(filter, cancellationToken: cancellationToken);

            var fileInfo = await cursor.SingleOrDefaultAsync(cancellationToken);
            if (fileInfo == null)
                return null;

            return new PageFileDocument(fileInfo);
        }

        public async Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return await files.OpenDownloadStreamAsync(GuidConverter.ToBytes(fileId, GuidRepresentation.Standard), cancellationToken: cancellationToken);
        }

        public Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return files.DeleteAsync(GuidConverter.ToBytes(fileId, GuidRepresentation.Standard), cancellationToken);
        }

        class FileBucket(IMongoDatabase database, GridFSBucketOptions options = null) : GridFSBucket<byte[]>(database, options)
        {
        }
    }
}