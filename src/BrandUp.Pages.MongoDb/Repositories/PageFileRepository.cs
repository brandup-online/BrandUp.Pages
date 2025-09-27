using BrandUp.Pages.Files;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
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

            var metadata = (new PageFileDocument.Metadata
            {
                ContentType = contentType,
                FileName = fileName,
                PageId = pageId
            }).ToBsonDocument();

            var uploadOptions = new GridFSUploadOptions
            {
                Metadata = metadata
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

            PageFileDocument.Metadata metadata;
            var serializer = BsonSerializer.LookupSerializer<PageFileDocument.Metadata>();
            using (var bsonReader = new BsonDocumentReader(fileInfo.Metadata))
            {
                var context = BsonDeserializationContext.CreateRoot(bsonReader, null);
                metadata = serializer.Deserialize(context);
            }

            return new PageFileDocument(fileInfo, metadata);
        }

        public async Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return await files.OpenDownloadStreamAsync(fileId, cancellationToken: cancellationToken);
        }

        public Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return files.DeleteAsync(fileId, cancellationToken);
        }

        class FileBucket(IMongoDatabase database, GridFSBucketOptions options = null) : GridFSBucket<Guid>(database, options)
        {
        }
    }
}