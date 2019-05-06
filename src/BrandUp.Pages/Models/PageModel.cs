using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace BrandUp.Pages.Models
{
    public class PageModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Title { get; set; }
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PageStatus Status { get; set; }
        public string Url { get; set; }
    }

    public enum PageStatus
    {
        Draft,
        Published
    }

    public class PageCreateModel
    {
        [Required]
        public string PageType { get; set; }
        [Required(AllowEmptyStrings = false), MaxLength(255)]
        public string Title { get; set; }
    }
}