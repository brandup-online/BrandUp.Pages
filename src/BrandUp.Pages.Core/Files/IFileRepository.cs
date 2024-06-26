﻿namespace BrandUp.Pages.Files
{
	public interface IFileRepository
	{
		Task<IFile> UploadFileAsync(Guid pageId, string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default);
		Task<IFile> FindFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default);
		Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default);
		Task DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);
	}
}