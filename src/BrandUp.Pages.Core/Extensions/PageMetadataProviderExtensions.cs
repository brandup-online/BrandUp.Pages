using System.Collections.Generic;

namespace BrandUp.Pages.Metadata
{
    public static class PageMetadataProviderExtensions
    {
        public static IEnumerable<PageMetadataProvider> GetDerivedMetadataWithHierarhy(this PageMetadataProvider pageMetadata, bool includeCurrent)
        {
            if (includeCurrent)
                yield return pageMetadata;

            foreach (var derivedPageMetadata in pageMetadata.DerivedTypes)
            {
                yield return derivedPageMetadata;

                foreach (var childDerivedPageMetadata in GetDerivedMetadataWithHierarhy(derivedPageMetadata, false))
                    yield return childDerivedPageMetadata;
            }
        }
    }
}