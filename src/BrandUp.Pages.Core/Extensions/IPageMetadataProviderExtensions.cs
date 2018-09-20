using System.Collections.Generic;

namespace BrandUp.Pages.Interfaces
{
    public static class IPageMetadataProviderExtensions
    {
        public static IEnumerable<IPageMetadataProvider> GetDerivedMetadataWithHierarhy(this IPageMetadataProvider pageMetadata, bool includeCurrent)
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