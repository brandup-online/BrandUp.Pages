using BrandUp.Pages.Services;

namespace BrandUp.Pages.Features
{
    public class ContentPageFeature
    {
        internal ContentPageFeature(IPage page)
        {
            Page = page;
        }

        public IPage Page { get; }
    }
}