using BrandUp.MongoDB;
using BrandUp.Pages.MongoDb.Documents;
using LandingWebSite.Identity;
using MongoDB.Driver;

namespace LandingWebSite.Models
{
    public class AppDbContext : MongoDbContext, BrandUp.Pages.MongoDb.IPagesDbContext
    {
        public IMongoCollection<IdentityUser> Users => GetCollection<IdentityUser>();
        public IMongoCollection<IdentityRole> Roles => GetCollection<IdentityRole>();

        public IMongoCollection<Blog.BlogPostDocument> BlogPosts => GetCollection<Blog.BlogPostDocument>();

        #region IPagesDbContext members

        public IMongoCollection<ContentDocument> Contents => GetCollection<ContentDocument>();
        public IMongoCollection<ContentCommitDocument> ContentCommits => GetCollection<ContentCommitDocument>();
        public IMongoCollection<ContentEditDocument> ContentEdits => GetCollection<ContentEditDocument>();

        public IMongoCollection<PageCollectionDocument> PageCollections => GetCollection<PageCollectionDocument>();
        public IMongoCollection<PageDocument> Pages => GetCollection<PageDocument>();
        public IMongoCollection<PageUrlDocument> PageUrls => GetCollection<PageUrlDocument>();
        public IMongoCollection<PageRecyclebinDocument> PageRecyclebin => GetCollection<PageRecyclebinDocument>();

        #endregion
    }
}