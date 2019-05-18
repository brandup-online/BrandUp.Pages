using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class PageEditSession : IPageEditSession
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid PageId { get; set; }
        public string ContentManagerId { get; set; }
    }

    [MongoDB.Document(CollectionName = "BrandUpPages.edits", CollectionContextType = typeof(PageEditSessionDocumentContextType))]
    public class PageEditSessionDocument : Document
    {
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid PageId { get; set; }
        [BsonRequired]
        public string ContentManagerId { get; set; }
        [BsonRequired]
        public BsonDocument Content { get; set; }
    }

    public class PageEditSessionDocumentContextType : MongoDB.MongoDbCollectionContext<PageEditSessionDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<PageEditSessionDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageEditSessionDocument>[] {
                new CreateIndexModel<PageEditSessionDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}