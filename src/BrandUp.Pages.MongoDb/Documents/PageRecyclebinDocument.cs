using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.recyclebin")]
    public class PageRecyclebinDocument : Document
    {
        [BsonRequired]
        public string WebsiteId { get; set; }
        [BsonRequired]
        public string TypeName { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid OwnCollectionId { get; set; }
        [BsonRequired]
        public string UrlPath { get; set; }
        [BsonRequired]
        public string Header { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public PageStatus Status { get; set; }
        public BsonDocument Content { get; set; }
    }
}