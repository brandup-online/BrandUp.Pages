using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Metadata
{
    public interface IPageMetadataManager
    {
        IEnumerable<PageMetadataProvider> MetadataProviders { get; }
        PageMetadataProvider FindPageMetadataByContentType(Type contentType);
        PageMetadataProvider FindPageMetadataByName(string name);
    }
}