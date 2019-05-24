using System.Collections.Generic;

namespace BrandUp.Pages.Models
{
    public class PageListModel
    {
        public List<string> Parents { get; set; }
        public List<PageCollectionModel> Collections { get; set; }
    }
}