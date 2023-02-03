using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.urls")]
    public class PageUrlDocument
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonDateTimeOptions(Representation = BsonType.DateTime)]
        public DateTime CreatedDate { get; set; }
        [BsonRequired]
        public string WebsiteId { get; set; }
        [BsonRequired]
        public string Path { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid? PageId { get; set; }
        public UrlRedirectDocument Redirect { get; set; }
    }

    public class UrlRedirectDocument
    {
        [BsonRequired]
        public bool IsPermament { get; set; }
        [BsonRequired]
        public string Path { get; set; }
    }
}