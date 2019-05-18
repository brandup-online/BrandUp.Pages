using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages
{
    public class ContentContext
    {
        public IPage Page { get; }
        public ContentExplorer Explorer { get; }
        public IServiceProvider Services { get; }
        public object Content => Explorer.Model;

        public ContentContext(IPage page, object pageContent, IServiceProvider services)
        {
            if (pageContent == null)
                throw new ArgumentNullException(nameof(pageContent));

            Page = page ?? throw new ArgumentNullException(nameof(page));
            Services = services ?? throw new ArgumentNullException(nameof(services));

            var contentMetadataManager = services.GetRequiredService<IContentMetadataManager>();

            Explorer = ContentExplorer.Create(contentMetadataManager, pageContent);
        }
        private ContentContext(ContentContext parent, ContentExplorer contentExplorer)
        {
            Page = parent.Page;
            Services = parent.Services;
            Explorer = contentExplorer;
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