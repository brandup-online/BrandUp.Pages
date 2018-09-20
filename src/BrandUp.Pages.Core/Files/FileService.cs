using BrandUp.Pages.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BrandUp.Pages.Files
{
    public class FileService : IFileService
    {
        private readonly IFileRepository repository;

        public FileService(IFileRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<IPageFile> FindFileByIdAsync(Guid fileId)
        {
            return repository.FindFileByIdAsync(fileId);
        }
        public async Task<IPageFile> UploadFileAsync(string fileName, Stream fileStream)
        {
            var fileId = await repository.UploadFileAsync(fileName, fileStream);
            return await repository.FindFileByIdAsync(fileId);
        }
        public Task<Stream> GetFileStreamAsync(Guid fileId)
        {
            return repository.GetFileStreamAsync(fileId);
        }
    }
}