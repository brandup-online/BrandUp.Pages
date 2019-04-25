using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb
{
    public interface IPagesDbContext
    {
        IMongoCollection<Documents.PageCollectionDocument> PageCollections { get; }
        IMongoCollection<Documents.PageDocument> Pages { get; }
        IMongoCollection<Documents.PageEditSessionDocument> PageEditSessions { get; }
    }
}