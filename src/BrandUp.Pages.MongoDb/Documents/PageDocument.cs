using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.pages")]
    public class PageDocument : Document, IPage
    {
        [BsonRequired]
        public string WebsiteId { get; set; }
        [BsonRequired]
        public string TypeName { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid OwnCollectionId { get; set; }
        public string ItemId { get; set; }
        public string ItemType { get; set; }
        [BsonRequired]
        public string UrlPath { get; set; }
        [BsonRequired]
        public string Header { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public PageStatus Status { get; set; }
        public PageSeoDocument Seo { get; set; }
        public bool IsPublished { get => Status == PageStatus.Published; }
        public int Order { get; set; }

    }

    public enum PageStatus
    {
        Draft,
        Published
    }

    public class PageSeoDocument
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Keywords { get; set; }
    }
}