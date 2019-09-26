using System;
using System.Text.Json.Serialization;

namespace BrandUp.Pages.Models
{
    public class PageModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Title { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PageStatus Status { get; set; }
        public string Url { get; set; }
    }

    public enum PageStatus
    {
        Draft,
        Published
    }

    public class BeginPageEditResult
    {
        public DateTime? CurrentDate { get; set; }
        public string Url { get; set; }
    }
}