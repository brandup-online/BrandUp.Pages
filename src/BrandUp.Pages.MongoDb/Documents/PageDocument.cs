using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.pages", CollectionContextType = typeof(PageDocumentContextType))]
    public class PageDocument : Document, IPage
    {
        [BsonRequired]
        public string WebSiteId { get; set; }
        [BsonRequired]
        public string TypeName { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid OwnCollectionId { get; set; }
        [BsonRequired]
        public string UrlPath { get; set; }
        [BsonRequired]
        public string Header { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public PageStatus Status { get; set; }
        public PageSeoDocument Seo { get; set; }
        public bool IsPublished { get => Status == PageStatus.Published; }
        public int Order { get; set; }
    }

    public enum PageStatus
    {
        Draft,
        Published
    }

    public class PageSeoDocument
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Keywords { get; set; }
    }

    public class PageDocumentContextType : MongoDB.MongoDbCollectionContext<PageDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var urlIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.UrlPath);
            var webSiteIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.WebSiteId);
            var textIndex = Builders<PageDocument>.IndexKeys.Text(it => it.Header).Text(it => it.UrlPath);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageDocument>[] {
                new CreateIndexModel<PageDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true }),
                new CreateIndexModel<PageDocument>(urlIndex, new CreateIndexOptions { Name = "Url", Unique = true }),
                new CreateIndexModel<PageDocument>(webSiteIndex, new CreateIndexOptions { Name = "WebSite" }),
                new CreateIndexModel<PageDocument>(textIndex, new CreateIndexOptions { Name = "TextSearch" })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}