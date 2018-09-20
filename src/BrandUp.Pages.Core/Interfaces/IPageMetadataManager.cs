using BrandUp.Pages.Content;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Interfaces
{
    public interface IPageMetadataManager
    {
        IEnumerable<IPageMetadataProvider> Types { get; }
        IPageMetadataProvider FindPageMetadataByContentType(Type contentType);
        IPageMetadataProvider FindPageMetadataByName(string name);
    }

    public interface IPageMetadataProvider
    {
        IContentMetadataProvider ContentMetadata { get; }
        string Name { get; }
        string Title { get; }
        string Description { get; }
        Type ContentType { get; }
        IPageMetadataProvider ParentMetadata { get; }
        IEnumerable<IPageMetadataProvider> DerivedTypes { get; }
        object CreatePageModel();
        string GetPageName(object pageModel);
    }
}