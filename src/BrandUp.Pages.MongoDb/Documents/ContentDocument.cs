using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.MongoCollection(CollectionName = "BrandUpPages.contents")]
    public class ContentDocument
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonRepresentation(BsonType.String), Obsolete]
        public Guid? PageId { get; set; }
        [BsonRequired]
        public string WebsiteId { get; set; }
        [BsonRequired]
        public string Key { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }
}