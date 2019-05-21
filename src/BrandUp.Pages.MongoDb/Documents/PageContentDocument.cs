using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.contents", CollectionContextType = typeof(PageContentDocumentContextType))]
    public class PageContentDocument : Document
    {
        [BsonRequired]
        public string PageId { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }

    public class PageContentDocumentContextType : MongoDB.MongoDbCollectionContext<PageContentDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<PageContentDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageContentDocument>[] {
                new CreateIndexModel<PageContentDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}