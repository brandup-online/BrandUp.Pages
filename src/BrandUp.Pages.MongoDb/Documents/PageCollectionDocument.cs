using BrandUp.Pages.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.Data.Documents
{
    public class PageCollectionDocument : Document, IPageCollection
    {
        [BsonRequired, CamelCase]
        public string Title { get; set; }
        [BsonRequired, CamelCase]
        public string PageTypeName { get; set; }
        [BsonRequired, BsonRepresentation(MongoDB.Bson.BsonType.String), CamelCase]
        public PageSortMode SortMode { get; set; }
        [BsonIgnoreIfNull, BsonRepresentation(MongoDB.Bson.BsonType.String), CamelCase]
        public Guid? PageId { get; set; }
    }
}