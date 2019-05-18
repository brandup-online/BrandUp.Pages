using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb
{
    public interface IPagesDbContext
    {
        IMongoDatabase Database { get; }
        IMongoCollection<Documents.PageCollectionDocument> PageCollections { get; }
        IMongoCollection<Documents.PageDocument> Pages { get; }
        IMongoCollection<Documents.ContentDocument> Contents { get; }
        IMongoCollection<Documents.PageEditSessionDocument> PageEditSessions { get; }
    }
}