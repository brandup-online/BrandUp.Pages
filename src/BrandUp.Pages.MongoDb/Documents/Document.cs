using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.Data.Documents
{
    public class Document
    {
        [BsonId, BsonRepresentation(MongoDB.Bson.BsonType.String), CamelCase]
        public Guid Id { get; set; }
        [BsonDateTimeOptions(DateOnly = false, Kind = DateTimeKind.Utc, Representation = MongoDB.Bson.BsonType.DateTime), CamelCase]
        public DateTime CreatedDate { get; set; }
    }
}