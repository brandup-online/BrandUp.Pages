using BrandUp.Pages.Content;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class ContentEdit : IContentEdit
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string WebsiteId { get; set; }
        public string ContentKey { get; set; }
        public string UserId { get; set; }
    }

    [MongoDB.MongoCollection(CollectionName = "BrandUpPages.edits")]
    public class ContentEditDocument : Document
    {
        [BsonRequired]
        public string WebsiteId { get; set; }
        [BsonRequired]
        public string ContentKey { get; set; }
        [BsonRequired]
        public string UserId { get; set; }
        [BsonRequired]
        public BsonDocument Content { get; set; }
    }
}