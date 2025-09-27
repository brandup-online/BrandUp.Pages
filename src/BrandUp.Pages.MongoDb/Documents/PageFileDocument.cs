using BrandUp.Pages.Files;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GridFS;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class PageFileDocument : IFile
    {
        public Guid Id { get; }
        public string ContentType { get; private set; }
        public string Name { get; private set; }
        public Guid PageId { get; private set; }

        internal PageFileDocument(Guid pageId, string fileName, string contentType)
        {
            Id = Guid.NewGuid();
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            Name = fileName ?? throw new ArgumentNullException(nameof(fileName));
            PageId = pageId;
        }

        internal PageFileDocument(GridFSFileInfo<Guid> fileInfo, Metadata metadata)
        {
            Id = fileInfo.Id;
            ContentType = metadata.ContentType;
            Name = metadata.FileName;
            PageId = metadata.PageId;
        }

        public class Metadata
        {
            [BsonElement("contentType")]
            public string ContentType { get; set; }
            [BsonElement("fileName")]
            public string FileName { get; set; }
            [BsonElement("pageId")]
            [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
            public Guid PageId { get; set; }
        }
    }
}