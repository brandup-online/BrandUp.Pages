using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Rendering
{
    public class PageContext
    {
        public object PageModel { get; set; }

        public PageContext(IPage page, Metadata.PageMetadataProvider pageMetadata, object pageModel)
        {
            PageModel = pageModel;
        }
    }
}