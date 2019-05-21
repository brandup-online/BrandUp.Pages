using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class PageEdit : IPageEdit
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid PageId { get; set; }
        public string UserId { get; set; }
    }

    [MongoDB.Document(CollectionName = "BrandUpPages.edits", CollectionContextType = typeof(PageEditSessionDocumentContextType))]
    public class PageEditDocument : Document
    {
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid PageId { get; set; }
        [BsonRequired]
        public string UserId { get; set; }
        [BsonRequired]
        public BsonDocument Content { get; set; }
    }

    public class PageEditSessionDocumentContextType : MongoDB.MongoDbCollectionContext<PageEditDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<PageEditDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var userIndex = Builders<PageEditDocument>.IndexKeys.Ascending(it => it.PageId).Ascending(it => it.UserId);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageEditDocument>[] {
                new CreateIndexModel<PageEditDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true }),
                new CreateIndexModel<PageEditDocument>(userIndex, new CreateIndexOptions { Name = "User", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}