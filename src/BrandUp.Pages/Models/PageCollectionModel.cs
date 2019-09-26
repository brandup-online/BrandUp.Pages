using BrandUp.Pages.Interfaces;
using System;
using System.Text.Json.Serialization;

namespace BrandUp.Pages.Models
{
    public class PageCollectionModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? PageId { get; set; }
        public string Title { get; set; }
        public string PageType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PageSortMode Sort { get; set; }
        public bool CustomSorting { get; set; }
        public string PageUrl { get; set; }
    }
}