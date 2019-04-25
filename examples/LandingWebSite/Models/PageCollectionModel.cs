using BrandUp.Pages.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;

namespace LandingWebSite.Models
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
    }

    public class PageCollectionCreateModel
    {
        [Required(AllowEmptyStrings = false), MaxLength(150)]
        public string Title { get; set; }
        [Required]
        public string PageType { get; set; }
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public PageSortMode Sort { get; set; }
    }

    public class PageCollectionUpdateModel
    {
        [Required(AllowEmptyStrings = false), MaxLength(150)]
        public string Title { get; set; }
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public PageSortMode Sort { get; set; }
    }
}