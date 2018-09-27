using BrandUp.Pages.Api.DataModels;
using System.Collections.Generic;

namespace BrandUp.Pages.Api.FormModels
{
    public class ContentListModel
    {
        public List<ContentListItemModel> Items { get; set; }
    }

    public class ContentListItemModel
    {
        public ContentMetadataModel Metadata { get; set; }
    }
}