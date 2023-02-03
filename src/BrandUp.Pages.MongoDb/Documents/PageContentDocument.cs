using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.contents")]
    public class PageContentDocument
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid PageId { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }
}