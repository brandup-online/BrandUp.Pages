using BrandUp.Pages.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.Data.Documents
{
    public class PageEditSession : IPageEditSession
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid PageId { get; set; }
        public string ContentManagerId { get; set; }
        public int ContentVersion { get; set; }
    }

    public class PageEditSessionDocument : Document
    {
        [BsonRequired, BsonRepresentation(MongoDB.Bson.BsonType.String), CamelCase]
        public Guid PageId { get; set; }
        [BsonRequired, CamelCase]
        public string ContentManagerId { get; set; }
        [BsonRequired, CamelCase]
        public PageContentDocument Content { get; set; }
    }
}