using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.contents", CollectionContextType = typeof(PageContentDocumentContextType))]
    public class PageContentDocument
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid PageId { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }

    public class PageContentDocumentContextType : MongoDB.MongoDbCollectionContext<PageContentDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var pageIdIndex = Builders<PageContentDocument>.IndexKeys.Ascending(it => it.PageId);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageContentDocument>[] {
                new CreateIndexModel<PageContentDocument>(pageIdIndex, new CreateIndexOptions { Name = "Page", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}