using BrandUp.Pages.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace BrandUp.Pages.Models
{
    public class PageCollectionModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? PageId { get; set; }
        public string Title { get; set; }
        public string PageType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PageSortMode Sort { get; set; }
        public string PageUrl { get; set; }
    }
}