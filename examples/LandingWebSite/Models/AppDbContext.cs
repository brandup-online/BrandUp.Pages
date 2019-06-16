using BrandUp.MongoDB;
using BrandUp.Pages.MongoDb.Documents;
using LandingWebSite.Identity;
using MongoDB.Driver;

namespace LandingWebSite.Models
{
    public class AppDbContext : MongoDbContext, BrandUp.Pages.MongoDb.IPagesDbContext
    {
        public AppDbContext(MongoDbContextOptions options) : base(options) { }

        public IMongoCollection<_migrations.MigrationVersionDocument> Migrations => GetCollection<_migrations.MigrationVersionDocument>();

        public IMongoCollection<IdentityUser> Users => GetCollection<IdentityUser>();
        public IMongoCollection<IdentityRole> Roles => GetCollection<IdentityRole>();

        #region IPagesDbContext members

        public IMongoCollection<PageCollectionDocument> PageCollections => GetCollection<PageCollectionDocument>();
        public IMongoCollection<PageDocument> Pages => GetCollection<PageDocument>();
        public IMongoCollection<PageContentDocument> Contents => GetCollection<PageContentDocument>();
        public IMongoCollection<PageEditDocument> PageEditSessions => GetCollection<PageEditDocument>();
        public IMongoCollection<PageRecyclebinDocument> PageRecyclebin => GetCollection<PageRecyclebinDocument>();
        public IMongoCollection<PageUrlDocument> PageUrls => GetCollection<PageUrlDocument>();

        #endregion
    }
}