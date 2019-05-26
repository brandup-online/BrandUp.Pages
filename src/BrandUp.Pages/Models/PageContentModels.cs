using System.Collections.Generic;

namespace BrandUp.Pages.Models
{
    public class PageContentForm
    {
        public PageContentPath Path { get; set; }
        public List<ContentFieldModel> Fields { get; } = new List<ContentFieldModel>();
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();
    }

    public class PageContentPath
    {
        public PageContentPath Parent { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int Index { get; set; }
    }

    public class ContentFieldModel
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public object Options { get; set; }
    }
}