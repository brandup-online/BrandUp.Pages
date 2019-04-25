using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class PageEditSession : IPageEditSession
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid PageId { get; set; }
        public string ContentManagerId { get; set; }
        public int ContentVersion { get; set; }
    }

    [BrandUp.MongoDB.Document]
    public class PageEditSessionDocument : Document
    {
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid PageId { get; set; }
        [BsonRequired]
        public string ContentManagerId { get; set; }
        [BsonRequired]
        public PageContentDocument Content { get; set; }
    }
}