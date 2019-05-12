using BrandUp.Pages.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Repositories
{
    public class FakePageFileRepository : IFileRepository
    {
        private readonly Dictionary<Guid, MemoryFile> files = new Dictionary<Guid, MemoryFile>();

        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public Task<Stream> GetFileStreamAsync(Guid fileId)
        {
            throw new NotImplementedException();
        }

        public Task<IFile> UploadFileAsync(string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            var file = new MemoryFile
            {
                Id = Guid.NewGuid(),
                Name = fileName,
                ContentType = contentType,
                Data = ReadFully(stream)
            };

            files.Add(file.Id, file);

            return Task.FromResult<IFile>(file);
        }

        public Task<IFile> FindFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            files.TryGetValue(fileId, out MemoryFile file);
            return Task.FromResult<IFile>(file);
        }

        public Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            if (!files.TryGetValue(fileId, out MemoryFile file))
                throw new Exception();

            return Task.FromResult<Stream>(new MemoryStream(file.Data));
        }

        public Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            if (!files.Remove(fileId))
                throw new Exception();

            return Task.CompletedTask;
        }

        private class MemoryFile : IFile
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string ContentType { get; set; }
            public byte[] Data { get; set; }
            public long Size => Data.Length;

        }
    }
}