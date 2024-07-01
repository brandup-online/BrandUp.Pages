﻿using MongoDB.Driver;

namespace BrandUp.Pages.MongoDb
{
    public interface IPagesDbContext
    {
        IMongoDatabase Database { get; }
        IMongoCollection<Documents.ContentDocument> Contents { get; }
        IMongoCollection<Documents.ContentCommitDocument> ContentCommits { get; }
        IMongoCollection<Documents.ContentEditDocument> ContentEdits { get; }
        IMongoCollection<Documents.PageCollectionDocument> PageCollections { get; }
        IMongoCollection<Documents.PageDocument> Pages { get; }
        IMongoCollection<Documents.PageRecyclebinDocument> PageRecyclebin { get; }
        IMongoCollection<Documents.PageUrlDocument> PageUrls { get; }
    }
}