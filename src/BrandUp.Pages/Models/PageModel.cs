using Newtonsoft.Json;
using System;

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

    public class PageNavigationModel
    {
        public Guid Id { get; set; }
        public Guid? ParentPageId { get; set; }
        public string Title { get; set; }
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PageStatus Status { get; set; }
        public string Url { get; set; }
        public Guid? EditId { get; set; }
    }
}