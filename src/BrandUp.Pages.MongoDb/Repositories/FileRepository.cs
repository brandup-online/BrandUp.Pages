using BrandUp.Pages.Content;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Repositories
{
    public class FileRepository : IFileRepository
    {
        public Task<IFileDocument> UploadFileAsync(string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IFileDocument> FindFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}