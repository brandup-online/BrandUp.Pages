using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.recyclebin", CollectionContextType = typeof(PageDeleteDocumentContextType))]
    public class PageRecyclebinDocument : Document
    {
        [BsonRequired]
        public string TypeName { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid OwnCollectionId { get; set; }
        [BsonRequired]
        public string UrlPath { get; set; }
        [BsonRequired]
        public string Title { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public PageStatus Status { get; set; }
        public BsonDocument Content { get; set; }
    }

    public class PageDeleteDocumentContextType : MongoDB.MongoDbCollectionContext<PageRecyclebinDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<PageRecyclebinDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageRecyclebinDocument>[] {
                new CreateIndexModel<PageRecyclebinDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}