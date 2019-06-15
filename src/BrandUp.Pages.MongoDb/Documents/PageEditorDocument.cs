using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.editors", CollectionContextType = typeof(PageEditorDocumentContextType))]
    public class PageEditorDocument : IPageEditor
    {
        public ObjectId Id { get; set; }
        [BsonDateTimeOptions(Representation = BsonType.DateTime)]
        public DateTime CreatedDate { get; set; }
        public int Version { get; set; }
        public string Email { get; set; }
        string IPageEditor.Id { get => Id.ToString(); }
    }

    public class PageEditorDocumentContextType : MongoDB.MongoDbCollectionContext<PageEditorDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<PageEditorDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var emailIndex = Builders<PageEditorDocument>.IndexKeys.Ascending(it => it.Email);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageEditorDocument>[] {
                new CreateIndexModel<PageEditorDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true }),
                new CreateIndexModel<PageEditorDocument>(emailIndex, new CreateIndexOptions { Name = "Email", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}