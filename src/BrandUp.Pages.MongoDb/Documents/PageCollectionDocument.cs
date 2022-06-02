using BrandUp.Pages.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.MongoDb.Documents
{
    [MongoDB.Document(CollectionName = "BrandUpPages.collections")]
    public class PageCollectionDocument : Document, IPageCollection
    {
        [BsonRequired]
        public string WebSiteId { get; set; }
        [BsonRequired]
        public string Title { get; set; }
        [BsonRequired]
        public string PageTypeName { get; set; }
        [BsonRequired, BsonRepresentation(BsonType.String)]
        public PageSortMode SortMode { get; set; }
        [BsonIgnoreIfNull, BsonRepresentation(BsonType.String)]
        public Guid? PageId { get; set; }
        public bool CustomSorting { get; set; }

        void IPageCollection.SetSortModel(PageSortMode sortMode)
        {
            SortMode = sortMode;
        }
        void IPageCollection.SetTitle(string newTitle)
        {
            Title = newTitle ?? throw new ArgumentNullException(nameof(newTitle));
        }
        public void SetCustomSorting(bool enabledCustomSorting)
        {
            CustomSorting = enabledCustomSorting;
        }
    }
}