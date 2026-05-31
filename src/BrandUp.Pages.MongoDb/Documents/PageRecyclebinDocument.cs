using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BrandUp.Pages.MongoDb.Documents
{
	[MongoDB.MongoCollection(CollectionName = "BrandUpPages.recyclebin")]
	public class PageRecyclebinDocument : Document
	{
		[BsonRequired]
		public string WebsiteId { get; set; } = null!;
		[BsonRequired]
		public string TypeName { get; set; } = null!;
		[BsonRequired, BsonRepresentation(BsonType.String)]
		public Guid OwnCollectionId { get; set; }
		[BsonRequired]
		public string UrlPath { get; set; } = null!;
		[BsonRequired]
		public string Header { get; set; } = null!;
		[BsonRequired, BsonRepresentation(BsonType.String)]
		public PageStatus Status { get; set; }
		public BsonDocument Content { get; set; } = null!;
	}
}