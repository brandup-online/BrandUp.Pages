using BrandUp.MongoDB;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb
{
	public class PagesDbContext : MongoDbContext, IPagesDbContext
	{
		public IMongoCollection<PageCollectionDocument> PageCollections => GetCollection<PageCollectionDocument>();
		public IMongoCollection<PageDocument> Pages => GetCollection<PageDocument>();
		public IMongoCollection<ContentEditDocument> ContentEdits => GetCollection<ContentEditDocument>();
		public IMongoCollection<ContentDocument> Contents => GetCollection<ContentDocument>();
		public IMongoCollection<PageRecyclebinDocument> PageRecyclebin => GetCollection<PageRecyclebinDocument>();
		public IMongoCollection<PageUrlDocument> PageUrls => GetCollection<PageUrlDocument>();
	}
}