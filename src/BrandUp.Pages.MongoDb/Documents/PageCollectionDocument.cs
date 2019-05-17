using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionContextType = typeof(PageCollectionDocumentContextType))]
    public class PageCollectionDocument : Document, IPageCollection
    {
        [BsonRequired]
        public string Title { get; set; }
        [BsonRequired]
        public string PageTypeName { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public PageSortMode SortMode { get; set; }
        [BsonIgnoreIfNull, BsonRepresentation(BsonType.String)]
        public Guid? PageId { get; set; }
    }

    public class PageCollectionDocumentContextType : MongoDB.MongoDbCollectionContext<PageCollectionDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<PageCollectionDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageCollectionDocument>[] {
                new CreateIndexModel<PageCollectionDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}