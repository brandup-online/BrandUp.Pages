using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BrandUp.Pages.Models
{
    public class AppClientModel
    {
        public string BaseUrl { get; set; }
        public AntiforgeryModel Antiforgery { get; set; }
        public NavigationClientModel Nav { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }

    public class AntiforgeryModel
    {
        public string HeaderName { get; set; }
        public string FormFieldName { get; set; }
    }

    public class NavigationClientModel
    {
        public bool EnableAdministration { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public IDictionary<string, object> Query { get; set; }
        public string ValidationToken { get; set; }
        public PageClientModel Page { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }

    public class PageClientModel
    {
        public string Title { get; set; }
        public string CssClass { get; set; }
        public string ScriptName { get; set; }
        public string CanonicalLink { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}