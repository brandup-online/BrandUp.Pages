using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.MongoDb.Documents
{
    [BrandUp.MongoDB.Document]
    public class PageCollectionDocument : Document, IPageCollection
    {
        [BsonRequired]
        public string Title { get; set; }
        [BsonRequired]
        public string PageTypeName { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public PageSortMode SortMode { get; set; }
        [BsonIgnoreIfNull, BsonRepresentation(BsonType.String)]
        public Guid? PageId { get; set; }
    }
}