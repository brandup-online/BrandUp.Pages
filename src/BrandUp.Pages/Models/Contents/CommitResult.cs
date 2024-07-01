namespace BrandUp.Pages.Models.Contents
{
    public class CommitResult
    {
        public bool IsSuccess { get; set; }
        public List<ContentValidationResult> Validation { get; set; }
    }

    public class ContentValidationResult
    {
        public string Path { get; set; }
        public int Index { get; set; }
        public string TypeName { get; set; }
        public string TypeTitle { get; set; }
        public List<string> Errors { get; set; }
        public List<FieldValidationResult> Fields { get; set; }
    }

    public class FieldValidationResult
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public List<string> Errors { get; set; }
    }
}