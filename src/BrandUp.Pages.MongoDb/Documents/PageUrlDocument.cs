using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.urls", CollectionContextType = typeof(PageUrlDocumentContextType))]
    public class PageUrlDocument
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonDateTimeOptions(Representation = BsonType.DateTime)]
        public DateTime CreatedDate { get; set; }
        [BsonRequired]
        public string WebSiteId { get; set; }
        [BsonRequired]
        public string Path { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid? PageId { get; set; }
        public UrlRedirectDocument Redirect { get; set; }
    }

    public class UrlRedirectDocument
    {
        [BsonRequired]
        public bool IsPermament { get; set; }
        [BsonRequired]
        public string Path { get; set; }
    }

    public class PageUrlDocumentContextType : MongoDB.MongoDbCollectionContext<PageUrlDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var pathIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.WebSiteId).Ascending(it => it.Path);
            var pageIdIndex = Builders<PageUrlDocument>.IndexKeys.Ascending(it => it.PageId);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageUrlDocument>[] {
                new CreateIndexModel<PageUrlDocument>(pathIndex, new CreateIndexOptions { Name = "Path", Unique = true }),
                new CreateIndexModel<PageUrlDocument>(pageIdIndex, new CreateIndexOptions { Name = "PageId", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}