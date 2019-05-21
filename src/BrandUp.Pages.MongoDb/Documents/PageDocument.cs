﻿using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.pages", CollectionContextType = typeof(PageDocumentContextType))]
    public class PageDocument : Document, IPage
    {
        [BsonRequired]
        public string TypeName { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public Guid OwnCollectionId { get; set; }
        [BsonRequired]
        public string UrlPath { get; set; }
        [BsonRequired]
        public string Title { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public PageStatus Status { get; set; }
        public bool IsPublished { get => Status == PageStatus.Published; }

        Task IPage.SetUrlAsync(string urlPath)
        {
            UrlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));
            Status = PageStatus.Published;

            return Task.CompletedTask;
        }
    }

    public enum PageStatus
    {
        Draft,
        Published
    }

    public class PageDocumentContextType : MongoDB.MongoDbCollectionContext<PageDocument>
    {
        protected override void OnSetupCollection(CancellationToken cancellationToken = default)
        {
            var versionIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.Id).Ascending(it => it.Version);
            var urlIndex = Builders<PageDocument>.IndexKeys.Ascending(it => it.UrlPath);
            var textIndex = Builders<PageDocument>.IndexKeys.Text(it => it.Title).Text(it => it.UrlPath);

            Collection.Indexes.CreateMany(new CreateIndexModel<PageDocument>[] {
                new CreateIndexModel<PageDocument>(versionIndex, new CreateIndexOptions { Name = "Version", Unique = true }),
                new CreateIndexModel<PageDocument>(urlIndex, new CreateIndexOptions { Name = "Url", Unique = true }),
                new CreateIndexModel<PageDocument>(textIndex, new CreateIndexOptions { Name = "TextSearch" })
            });

            base.OnSetupCollection(cancellationToken);
        }
    }
}