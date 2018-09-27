using BrandUp.Pages.Builder;
using BrandUp.Pages.Data.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;

namespace BrandUp.Pages.Data
{
    public class WebSiteContext
    {
        public const string MongoCollectionName_PageCollection = "PageCollections";
        public const string MongoCollectionName_Page = "Pages";
        public const string MongoCollectionName_PageEditSession = "PageEditSessions";
        public const string MongoCollectionName_Files = "Files";
        private MongoClient client;
        private IMongoDatabase database;
        private readonly FileBucket pageFiles;

        public MongoClient Client => client;
        public IMongoDatabase Database => database;
        public FileBucket Files => pageFiles;

        public WebSiteContext(IOptions<MongoDbOptions> options)
        {
            client = new MongoClient(options.Value.ConnectionString);
            database = client.GetDatabase(options.Value.DatabaseName);
            pageFiles = new FileBucket(database, new GridFSBucketOptions { BucketName = MongoCollectionName_Files, DisableMD5 = false });
        }

        private IMongoCollection<T> Collection<T>(string collectionName)
        {
            if (collectionName == null)
                throw new ArgumentNullException(nameof(collectionName));

            return database.GetCollection<T>(collectionName);
        }

        public IMongoCollection<PageDocument> GetPageDocuments()
        {
            return Collection<PageDocument>(MongoCollectionName_Page);
        }
        public IMongoCollection<PageCollectionDocument> GetPageCollectionDocuments()
        {
            return Collection<PageCollectionDocument>(MongoCollectionName_PageCollection);
        }
        public IMongoCollection<PageEditSessionDocument> GetPageEditSessionDocuments()
        {
            return Collection<PageEditSessionDocument>(MongoCollectionName_PageEditSession);
        }
    }

    public class FileBucket : GridFSBucket<Guid>
    {
        public FileBucket(IMongoDatabase database, GridFSBucketOptions options = null) : base(database, options)
        {

        }
    }
}