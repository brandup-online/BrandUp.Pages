namespace BrandUp.Pages.Rendering
{
    public class ContentContext
    {
        public object ContentModel { get; set; }

        public ContentContext(Content.ContentMetadataProvider contentMetadata, object pageModel)
        {
            ContentModel = pageModel;
        }
    }
}