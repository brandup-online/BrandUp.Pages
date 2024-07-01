using BrandUp.Pages.Content;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages
{
    public class ContentContext
    {
        /// <summary>
        /// Ключ контента.
        /// </summary>
        public string Key { get; }
        public ContentExplorer Explorer { get; }
        public IServiceProvider Services { get; }
        public object Content => Explorer.Model;
        public Guid? EditId { get; }
        public bool IsDesigner => EditId.HasValue;

        public ContentContext(string key, object contentModel, IServiceProvider services, IContentEdit contentEdit)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(contentModel);
            ArgumentNullException.ThrowIfNull(services);

            Key = key;
            Services = services;

            var contentMetadataManager = services.GetRequiredService<ContentMetadataManager>();

            Explorer = ContentExplorer.Create(contentMetadataManager, contentModel);
            EditId = contentEdit?.Id;
        }

        ContentContext(ContentContext parent, ContentExplorer contentExplorer)
        {
            Key = parent.Key;
            Services = parent.Services;
            Explorer = contentExplorer;
            EditId = parent.EditId;
        }

        public ContentContext Navigate(string path)
        {
            var explorer = Explorer.Navigate(path);
            if (explorer == null)
                return null;

            return new ContentContext(this, explorer);
        }
    }
}