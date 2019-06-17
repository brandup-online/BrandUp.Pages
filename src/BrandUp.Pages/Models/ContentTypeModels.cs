using System.Collections.Generic;

namespace BrandUp.Pages.Models
{
    public class ContentTypeModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
    }

    public class ContentTypeListModel
    {
        public List<string> Parents { get; set; }
    }

    public class ContentTypeItemModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public bool IsAbstract { get; set; }
    }
}