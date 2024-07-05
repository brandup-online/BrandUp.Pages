namespace BrandUp.Pages.Models.Contents
{
    public abstract class ContentModelResult
    {
        public FieldValueResult FieldValue { get; set; }
    }

    public class AddContentResult : ContentModelResult
    {
        public List<ContentModel> Content { get; set; }
    }
}