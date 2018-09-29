using System.Collections.Generic;

namespace BrandUp.Pages.Content.Views
{
    public interface IContentViewResolver
    {
        IContentViewConfiguration GetViewsConfiguration(ContentMetadataProvider contentMetadata);
    }
    public interface IContentViewConfiguration
    {
        IList<IContentViewDefinitiuon> Views { get; }
        string DefaultViewName { get; }
    }
    public interface IContentViewDefinitiuon
    {
        string Name { get; }
        string Title { get; }
        string Description { get; }
    }
}