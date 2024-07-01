using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.MongoCollection(CollectionName = "BrandUpPages.contents.commit")]
    public class ContentCommitDocument
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public ObjectId Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public ObjectId? SourceId { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid ContentId { get; set; }
        [BsonRequired, BsonDateTimeOptions(Kind = DateTimeKind.Utc, Representation = BsonType.DateTime)]
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        [BsonRequired]
        public string Type { get; set; }
        [BsonRequired]
        public string Title { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }
}