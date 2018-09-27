using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.Data.Documents
{
    public class Page : IPage
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TypeName { get; set; }
        public Guid OwnCollectionId { get; set; }
        public string UrlPath { get; set; }
        public int ContentVersion { get; set; }
    }

    public class PageDocument : Document
    {
        [BsonRequired, CamelCase]
        public string PageType { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String), CamelCase]
        public Guid OwnCollectionId { get; set; }
        [BsonIgnoreIfNull, CamelCase]
        public string UrlPath { get; set; }
        [BsonRequired, CamelCase]
        public PageContentDocument Content { get; set; }
    }

    public class PageContentDocument
    {
        [BsonRequired, CamelCase]
        public int Version { get; set; }
        [BsonRequired, CamelCase]
        public BsonDocument Data { get; set; }
    }
}