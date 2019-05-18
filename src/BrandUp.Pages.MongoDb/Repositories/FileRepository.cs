using BrandUp.Pages.Content.Files;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly FileBucket files;

        public FileRepository(IPagesDbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            files = new FileBucket(dbContext.Database, new GridFSBucketOptions { BucketName = "BrandUpPages", DisableMD5 = false });
        }

        public async Task<IFile> UploadFileAsync(string entryId, string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            var fileDoc = new FileDocument(entryId, fileName, contentType);

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
    }

    public class FileBucket : GridFSBucket<Guid>
    {
        public FileBucket(IMongoDatabase database, GridFSBucketOptions options = null) : base(database, options) { }
    }

    public class FileDocument : IFile
    {
        public IDictionary<string, object> Data { get; private set; }

        public Guid Id { get; }
        public string ContentType { get => (string)Data["contentType"]; private set => Data["contentType"] = value; }
        public string FileName { get => (string)Data["fileName"]; private set => Data["fileName"] = value; }
        public string EntryId { get => (string)Data["pageId"]; private set => Data["pageId"] = value; }

        public FileDocument(string entryId, string fileName, string contentType)
        {
            Data = new Dictionary<string, object>();
            Id = Guid.NewGuid();
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            EntryId = entryId ?? throw new ArgumentNullException(nameof(entryId));
        }
        public FileDocument(GridFSFileInfo<Guid> fileInfo)
        {
            Id = fileInfo.Id;
            Data = MongoDbHelper.BsonDocumentToDictionary(fileInfo.Metadata);
        }
    }
}