namespace BrandUp.Pages.Models.Contents
{
    public class BeginPageEditResult
    {
        public Guid EditId { get; set; }
        public DateTime? CurrentDate { get; set; }
        public List<ContentModel> Content { get; set; }
    }

    public class ContentModel
    {
        public string Parent { get; set; }
        public string Path { get; set; }
        public int Index { get; set; }
        public string TypeName { get; set; }
        public string TypeTitle { get; set; }
        public List<Field> Fields { get; set; }

        public class Field
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public bool IsRequired { get; set; }
            public object Options { get; set; }
            public object Value { get; set; }
            public List<string> Errors { get; set; }
        }
    }
}