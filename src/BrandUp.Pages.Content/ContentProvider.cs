using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages.Content
{
    public class ContentProvider<TEntry>
        where TEntry : class, IContentEntry
    {
        public TEntry Entry { get; }
        public ContentExplorer Explorer { get; }
        public IServiceProvider Services { get; }
        public object Model => Explorer.Model;

        private ContentProvider(TEntry page, object model, IServiceProvider services)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            Entry = page ?? throw new ArgumentNullException(nameof(page));
            Services = services ?? throw new ArgumentNullException(nameof(services));

            var contentMetadataManager = services.GetRequiredService<IContentMetadataManager>();

            Explorer = ContentExplorer.Create(contentMetadataManager, model);
        }
        private ContentProvider(ContentProvider<TEntry> parent, ContentExplorer contentExplorer)
        {
            Entry = parent.Entry;
            Services = parent.Services;
            Explorer = contentExplorer;
        }

        public static ContentProvider<TEntry> Create(TEntry entry, object model, IServiceProvider services)
        {
            return new ContentProvider<TEntry>(entry, model, services);
        }
        public ContentProvider<TEntry> Navigate(string modelPath)
        {
            var explorer = Explorer.Navigate(modelPath);
            if (explorer == null)
                return null;

            return new ContentProvider<TEntry>(this, explorer);
        }
    }
}