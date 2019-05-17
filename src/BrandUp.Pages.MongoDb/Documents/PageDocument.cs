using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class Page : IPage
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TypeName { get; set; }
        public Guid OwnCollectionId { get; set; }
        public string UrlPath { get; set; }
        public string Title { get; set; }
    }

    [MongoDB.Document(CollectionContextType = typeof(PageDocumentContextType))]
    public class PageDocument : Document
    {
        [BsonRequired]
        public string PageType { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid OwnCollectionId { get; set; }
        [BsonIgnoreIfNull]
        public string UrlPath { get; set; }
        [BsonRequired]
        public string Title { get; set; }
        [BsonRequired]
        public BsonDocument Content { get; set; }
    }

    public class PageDocumentContextType : MongoDB.MongoDbCollectionContext<PageDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var urlIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.UrlPath);
            var textIndex = Builders<PageDocument>.IndexKeys.Text(it => it.Title).Text(it => it.UrlPath);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageDocument>[] {
                new CreateIndexModel<PageDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true }),
                new CreateIndexModel<PageDocument>(urlIndex, new CreateIndexOptions { Name = "Url", Unique = true }),
                new CreateIndexModel<PageDocument>(textIndex, new CreateIndexOptions { Name = "TextSearch" })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}