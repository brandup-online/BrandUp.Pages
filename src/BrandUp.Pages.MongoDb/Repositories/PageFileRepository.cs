using BrandUp.Pages.Files;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class PageFileRepository : IFileRepository
    {
        private readonly FileBucket files;

        public PageFileRepository(IPagesDbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            files = new FileBucket(dbContext.Database, new GridFSBucketOptions { BucketName = "BrandUpPages", DisableMD5 = false });
        }

        public async Task<IFile> UploadFileAsync(Guid pageId, string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            var fileDoc = new PageFileDocument(pageId, fileName, contentType);

            var uploadOptions = new GridFSUploadOptions
            {
                Metadata = MongoDbHelper.DictionaryToBsonDocument(fileDoc.Data),
                DisableMD5 = false
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

            return new PageFileDocument(fileInfo);
        }
        public async Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return await files.OpenDownloadStreamAsync(fileId, cancellationToken: cancellationToken);
        }
        public Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return files.DeleteAsync(fileId, cancellationToken);
        }

        class FileBucket : GridFSBucket<Guid>
        {
            public FileBucket(IMongoDatabase database, GridFSBucketOptions options = null) : base(database, options) { }
        }
    }
}