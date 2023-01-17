using BrandUp.MongoDB;
using BrandUp.Pages.MongoDb.Documents;
using LandingWebSite.Identity;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;

namespace LandingWebSite.Models
{
    public class AppDbContext : MongoDbContext, BrandUp.Pages.MongoDb.IPagesDbContext
    {
        public AppDbContext(MongoDbContextOptions options) : base(options) { }

        public IMongoCollection<IdentityUser> Users => GetCollection<IdentityUser>();
        public IMongoCollection<IdentityRole> Roles => GetCollection<IdentityRole>();

        public IMongoCollection<BlogPostDocument> BlogPosts => GetCollection<BlogPostDocument>();

        #region IPagesDbContext members

        public IMongoCollection<PageCollectionDocument> PageCollections => GetCollection<PageCollectionDocument>();
        public IMongoCollection<PageDocument> Pages => GetCollection<PageDocument>();
        public IMongoCollection<PageContentDocument> Contents => GetCollection<PageContentDocument>();
        public IMongoCollection<PageEditDocument> PageEditSessions => GetCollection<PageEditDocument>();
        public IMongoCollection<PageRecyclebinDocument> PageRecyclebin => GetCollection<PageRecyclebinDocument>();
        public IMongoCollection<PageUrlDocument> PageUrls => GetCollection<PageUrlDocument>();

        #endregion
    }

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