namespace BrandUp.Pages.Models.Contents
{
    public class BeginContentEditResult
    {
        public Guid EditId { get; set; }
        public bool Exist { get; set; }
    }

    public class GetContentEditResult
    {
        public string ContentKey { get; set; }
        public string Path { get; set; }
        public List<ContentModel> Contents { get; set; }
    }

    public class ContentModel
    {
        public string ParentPath { get; set; }
        public string ParentField { get; set; }
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