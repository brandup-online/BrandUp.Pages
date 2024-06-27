using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.MongoCollection(CollectionName = "BrandUpPages.contents")]
    public class ContentDocument
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonRequired]
        public string WebsiteId { get; set; }
        [BsonRequired]
        public string Key { get; set; }
        [BsonRequired]
        public string Type { get; set; }
        [BsonRequired]
        public string Title { get; set; }
        [BsonRepresentation(BsonType.String)]
        public ObjectId? Prev { get; set; }
        [BsonRepresentation(BsonType.String), BsonRequired]
        public ObjectId Version { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }
}