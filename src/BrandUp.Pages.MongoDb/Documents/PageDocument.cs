using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class Page : IPage
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TypeName { get; set; }
        public Guid OwnCollectionId { get; set; }
        public string UrlPath { get; set; }
        public int ContentVersion { get; set; }
        public string Title { get; set; }
    }

    [BrandUp.MongoDB.Document]
    public class PageDocument : Document
    {
        [BsonRequired]
        public string PageType { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid OwnCollectionId { get; set; }
        [BsonIgnoreIfNull]
        public string UrlPath { get; set; }
        public string Title { get; set; }
        [BsonRequired]
        public PageContentDocument Content { get; set; }
    }

    public class PageContentDocument
    {
        [BsonRequired]
        public int Version { get; set; }
        [BsonRequired]
        public BsonDocument Data { get; set; }
    }
}