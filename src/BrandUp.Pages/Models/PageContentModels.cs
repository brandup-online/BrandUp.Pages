using System.Collections.Generic;

namespace BrandUp.Pages.Models
{
    public class PageContentForm
    {
        public string Path { get; set; }
        public List<ContentFieldModel> Fields { get; } = new List<ContentFieldModel>();
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();
    }

    public class ContentFieldModel
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public object Options { get; set; }
    }
}