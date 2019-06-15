using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb
{
    public interface IPagesDbContext
    {
        IMongoDatabase Database { get; }
        IMongoCollection<Documents.PageCollectionDocument> PageCollections { get; }
        IMongoCollection<Documents.PageDocument> Pages { get; }
        IMongoCollection<Documents.PageContentDocument> Contents { get; }
        IMongoCollection<Documents.PageEditDocument> PageEditSessions { get; }
        IMongoCollection<Documents.PageRecyclebinDocument> PageRecyclebin { get; }
        IMongoCollection<Documents.PageUrlDocument> PageUrls { get; }
        IMongoCollection<Documents.ContentEditorDocument> ContentEditors { get; }
    }
}