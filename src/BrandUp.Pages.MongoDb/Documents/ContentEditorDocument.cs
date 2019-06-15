using BrandUp.Pages.Administration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.editors", CollectionContextType = typeof(ContentEditorDocumentContextType))]
    public class ContentEditorDocument : Administration.IContentEditor
    {
        public ObjectId Id { get; set; }
        [BsonDateTimeOptions(Representation = BsonType.DateTime)]
        public DateTime CreatedDate { get; set; }
        public int Version { get; set; }
        public string Email { get; set; }
        string IContentEditor.Id { get => Id.ToString(); }
    }

    public class ContentEditorDocumentContextType : MongoDB.MongoDbCollectionContext<ContentEditorDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<ContentEditorDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var emailIndex = Builders<ContentEditorDocument>.IndexKeys.Ascending(it => it.Email);

            Collection.Indexes.CreateMany(new CreateIndexModel<ContentEditorDocument>[] {
                new CreateIndexModel<ContentEditorDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true }),
                new CreateIndexModel<ContentEditorDocument>(emailIndex, new CreateIndexOptions { Name = "Email", Unique = true })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}