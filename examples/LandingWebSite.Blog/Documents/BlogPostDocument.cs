using BrandUp.MongoDB;
using MongoDB.Bson.Serialization.Attributes;

namespace LandingWebSite.Blog.Documents
{
    [Document(CollectionName = "Blog.Post")]
    public class BlogPostDocument
    {
        [BsonId, BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid Id { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc, Representation = MongoDB.Bson.BsonType.DateTime)]
        public DateTime CreatedDate { get; set; }
        [BsonRequired]
        public string Title { get; set; }
    }
}