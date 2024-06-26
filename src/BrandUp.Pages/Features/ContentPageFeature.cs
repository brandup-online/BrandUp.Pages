using BrandUp.Pages.Interfaces;

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