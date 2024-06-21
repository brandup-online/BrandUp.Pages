using BrandUp.Pages.Files;
using MongoDB.Driver.GridFS;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class FileDocument : IFile
    {
        public IDictionary<string, object> Data { get; private set; }

        public Guid Id { get; }
        public string ContentType { get => (string)Data["contentType"]; private set => Data["contentType"] = value; }
        public string Name { get => (string)Data["fileName"]; private set => Data["fileName"] = value; }
        public string WebsiteId { get => (string)Data["websiteId"]; private set => Data["websiteId"] = value; }
        public string ContentKey { get => (string)Data["contentKey"]; private set => Data["contentKey"] = value; }

        public FileDocument(string websiteId, string contentKey, string fileName, string contentType)
        {
            Data = new Dictionary<string, object>();
            Id = Guid.NewGuid();
            WebsiteId = websiteId ?? throw new ArgumentNullException(nameof(websiteId));
            ContentKey = contentKey ?? throw new ArgumentNullException(nameof(contentKey));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            Name = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public FileDocument(GridFSFileInfo<Guid> fileInfo)
        {
            Id = fileInfo.Id;
            Data = MongoDbHelper.BsonDocumentToDictionary(fileInfo.Metadata);
        }
    }
}