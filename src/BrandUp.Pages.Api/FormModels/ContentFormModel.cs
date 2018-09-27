using System.Collections.Generic;

namespace BrandUp.Pages.Api.FormModels
{
    public class ContentFormModel
    {
        public DataModels.ContentMetadataModel Metadata { get; set; }
        public List<ContentFormFieldModel> Fields { get; set; }
    }

    public class ContentFormFieldModel
    {
        public string FieldType { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public bool Required { get; set; }
        public object Options { get; set; }
        public object Value { get; set; }
    }
}