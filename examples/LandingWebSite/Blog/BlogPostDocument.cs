using BrandUp.MongoDB;
using BrandUp.Pages.Content.Items;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LandingWebSite.Blog
{
    [MongoCollection(CollectionName = "Blog.Post")]
    public class BlogPostDocument : IItemContent
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public ObjectId Id { get; set; }
        [BsonRequired, BsonDateTimeOptions(Kind = DateTimeKind.Utc, Representation = BsonType.DateTime)]
        public DateTime CreatedDate { get; set; }
        [BsonRequired]
        public string Title { get; set; }

        #region IItemContent members

        string IItemContent.ItemId => Id.ToString();

        #endregion
    }
}