using BrandUp.Pages.Content;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.MongoCollection(CollectionName = "BrandUpPages.contents")]
    public class ContentDocument : IContent
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonRequired]
        public string WebsiteId { get; set; }
        [BsonRequired]
        public string Key { get; set; }
        /// <summary>
        /// ID последнего коммита контента.
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public ObjectId? CommitId { get; set; }

        string IContent.CommitId => CommitId?.ToString();
    }
}