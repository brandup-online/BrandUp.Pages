using BrandUp.Pages.Files;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class PageFileDocument : IFile
    {
        public IDictionary<string, object> Data { get; private set; }

        public Guid Id { get; }
        public string ContentType { get => (string)Data["contentType"]; private set => Data["contentType"] = value; }
        public string Name { get => (string)Data["fileName"]; private set => Data["fileName"] = value; }
        public Guid PageId { get => new((byte[])Data["pageId"]); }

        public PageFileDocument(Guid pageId, string fileName, string contentType)
        {
            Data = new Dictionary<string, object>();
            Id = Guid.NewGuid();
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            Name = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Data["pageId"] = GuidConverter.ToBytes(pageId, GuidRepresentation.Standard);
        }

        public PageFileDocument(GridFSFileInfo<byte[]> fileInfo)
        {
            Id = GuidConverter.FromBytes(fileInfo.Id, GuidRepresentation.Standard);
            Data = MongoDbHelper.BsonDocumentToDictionary(fileInfo.Metadata);
        }
    }
}