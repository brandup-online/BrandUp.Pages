using BrandUp.Pages.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BrandUp.Pages.Data.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly FileBucket gridFS;

        public FileRepository(WebSiteContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            gridFS = dbContext.Files;
        }

        public async Task<IPageFile> FindFileByIdAsync(Guid fileId)
        {
            var filter = Builders<GridFSFileInfo<Guid>>.Filter.Eq(info => info.Id, fileId);
            var cursor = await gridFS.FindAsync(filter);

            var fileInfo = await cursor.SingleOrDefaultAsync();
            if (fileInfo != null)
            {
                return new PageFile
                {
                    Id = fileInfo.Id,
                    Name = fileInfo.Filename,
                    Size = fileInfo.Length,
                    UploadDateTime = fileInfo.UploadDateTime
                };
            }

            return null;
        }

        public async Task<Guid> UploadFileAsync(string fileName, Stream fileStream)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            var fileId = Guid.NewGuid();

            await gridFS.UploadFromStreamAsync(fileId, fileName, fileStream, new GridFSUploadOptions { DisableMD5 = false });

            return fileId;
        }
        public async Task<Stream> GetFileStreamAsync(Guid fileId)
        {
            var stream = await gridFS.OpenDownloadStreamAsync(fileId);
            return stream;
        }

        private class PageFile : IPageFile
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public long Size { get; set; }
            public DateTime UploadDateTime { get; set; }
        }
    }
}