using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.contents", CollectionContextType = typeof(ContentDocumentContextType))]
    public class ContentDocument : Document
    {
        [BsonRequired]
        public string PageId { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }

    public class ContentDocumentContextType : MongoDB.MongoDbCollectionContext<ContentDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<ContentDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);

            Collection.Indexes.CreateMany(new CreateIndexModel<ContentDocument>[] {
                new CreateIndexModel<ContentDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}