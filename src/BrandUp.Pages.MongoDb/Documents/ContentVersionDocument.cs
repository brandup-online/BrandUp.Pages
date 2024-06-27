using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.MongoCollection(CollectionName = "BrandUpPages.contents.version")]
    public class ContentVersionDocument
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public ObjectId VersionId { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid ContentId { get; set; }
        [BsonRequired, BsonDateTimeOptions(Kind = DateTimeKind.Utc, Representation = BsonType.DateTime)]
        public DateTime Date { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }
}