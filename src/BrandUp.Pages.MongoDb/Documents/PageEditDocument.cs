﻿using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.MongoDb.Documents
{
    public class PageEdit : IPageEdit
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid PageId { get; set; }
        public string UserId { get; set; }
    }

    [MongoDB.Document(CollectionName = "BrandUpPages.edits")]
    public class PageEditDocument : Document
    {
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid PageId { get; set; }
        [BsonRequired]
        public string UserId { get; set; }
        [BsonRequired]
        public BsonDocument Content { get; set; }
    }
}