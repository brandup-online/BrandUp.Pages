using BrandUp.Pages.Content.Files;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class PageFileDocument : IFile
    {
        public IDictionary<string, object> Data { get; private set; }

        public Guid Id { get; }
        public string ContentType { get => (string)Data["contentType"]; private set => Data["contentType"] = value; }
        public string FileName { get => (string)Data["fileName"]; private set => Data["fileName"] = value; }
        public string EntryId { get => (string)Data["pageId"]; private set => Data["pageId"] = value; }

        public PageFileDocument(string entryId, string fileName, string contentType)
        {
            Data = new Dictionary<string, object>();
            Id = Guid.NewGuid();
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            EntryId = entryId ?? throw new ArgumentNullException(nameof(entryId));
        }
        public PageFileDocument(GridFSFileInfo<Guid> fileInfo)
        {
            Id = fileInfo.Id;
            Data = MongoDbHelper.BsonDocumentToDictionary(fileInfo.Metadata);
        }
    }
}