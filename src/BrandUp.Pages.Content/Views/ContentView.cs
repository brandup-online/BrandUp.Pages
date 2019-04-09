using System;

namespace BrandUp.Pages.Content.Views
{
    public class ContentView
    {
        public ContentMetadataProvider ContentMetadata { get; }
        public string Name { get; }
        public string Title { get; }
        public string Description { get; }
        public string CssClass { get; }

        public ContentView(ContentMetadataProvider contentMetadata, IContentViewDefinitiuon viewDefinitiuon)
        {
            ContentMetadata = contentMetadata ?? throw new ArgumentNullException(nameof(contentMetadata));

            if (viewDefinitiuon == null)
                throw new ArgumentNullException(nameof(viewDefinitiuon));
            if (string.IsNullOrWhiteSpace(viewDefinitiuon.Name))
                throw new ArgumentException($"Value {nameof(viewDefinitiuon.Name)} is empty or null.");

            Name = string.Concat(contentMetadata.Name, ".", viewDefinitiuon.Name);
            Title = viewDefinitiuon.Title ?? Name;
            Description = viewDefinitiuon.Description;
            CssClass = viewDefinitiuon.CssClass;
        }
    }
}