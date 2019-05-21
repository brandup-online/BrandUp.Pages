using BrandUp.MongoDB;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb
{
    public class PagesDbContext : MongoDbContext, IPagesDbContext
    {
        public PagesDbContext(MongoDbContextOptions options) : base(options) { }

        public IMongoCollection<PageCollectionDocument> PageCollections => GetCollection<PageCollectionDocument>();
        public IMongoCollection<PageDocument> Pages => GetCollection<PageDocument>();
        public IMongoCollection<PageEditSessionDocument> PageEditSessions => GetCollection<PageEditSessionDocument>();
        public IMongoCollection<PageContentDocument> Contents => GetCollection<PageContentDocument>();
    }
}