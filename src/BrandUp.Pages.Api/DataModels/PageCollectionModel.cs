using BrandUp.Pages.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace BrandUp.Pages.Api.DataModels
{
    public class PageCollectionModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string PageTypeName { get; set; }
        public Guid? PageId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PageSortMode PageSort { get; set; }
    }
}