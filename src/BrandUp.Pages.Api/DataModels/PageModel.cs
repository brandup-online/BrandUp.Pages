using System;

namespace BrandUp.Pages.Api.DataModels
{
    public class PageModel
    {
        public Guid Id { get; set; }
        public PageTypeModel Type { get; set; }
        public PageCollectionModel Collection { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}