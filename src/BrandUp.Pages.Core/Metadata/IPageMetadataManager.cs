using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Metadata
{
    public interface IPageMetadataManager
    {
        IEnumerable<PageMetadataProvider> GetAllMetadata();
        PageMetadataProvider FindPageMetadataByContentType(Type contentType);
        PageMetadataProvider FindPageMetadataByName(string name);
    }
}