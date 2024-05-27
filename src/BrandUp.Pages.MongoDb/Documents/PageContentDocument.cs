using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.MongoCollection(CollectionName = "BrandUpPages.contents")]
    public class PageContentDocument
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid? PageId { get; set; }
        public string Url { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }
}