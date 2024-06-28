using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.MongoCollection(CollectionName = "BrandUpPages.contents.edit")]
    public class ContentEditDocument : Document
    {
        [BsonRequired]
        public string WebsiteId { get; set; }
        [BsonRequired]
        public string ContentKey { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid ContentId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public ObjectId? BaseCommitId { get; set; }
        [BsonRequired]
        public string UserId { get; set; }
        [BsonRequired]
        public BsonDocument Content { get; set; }
    }
}