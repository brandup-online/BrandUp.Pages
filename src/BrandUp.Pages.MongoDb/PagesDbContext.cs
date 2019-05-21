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
        public IMongoCollection<PageEditDocument> PageEditSessions => GetCollection<PageEditDocument>();
        public IMongoCollection<PageContentDocument> Contents => GetCollection<PageContentDocument>();
        public IMongoCollection<PageRecyclebinDocument> PageRecyclebin => GetCollection<PageRecyclebinDocument>();
    }
}