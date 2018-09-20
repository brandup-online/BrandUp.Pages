﻿using BrandUp.Pages.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BrandUp.Pages.Data.Repositories
{
    public class FakePageFileRepository : IFileRepository
    {
        private readonly Dictionary<Guid, IPageFile> files = new Dictionary<Guid, IPageFile>();

        public Task<IPageFile> FindFileByIdAsync(Guid fileId)
        {
            files.TryGetValue(fileId, out IPageFile file);
            return Task.FromResult(file);
        }

        public Task<Guid> UploadFileAsync(string fileName, Stream fileStream)
        {
            var file = new PageFile
            {
                Id = Guid.NewGuid(),
                Name = fileName,
                Data = ReadFully(fileStream)
            };

            files.Add(file.Id, file);

            return Task.FromResult(file.Id);
        }

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

        private class PageFile : IPageFile
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public byte[] Data { get; set; }
            public long Size => Data.Length;
        }
    }
}