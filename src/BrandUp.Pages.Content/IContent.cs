using System;

namespace BrandUp.Pages.Content
{
    public interface IContent
    {
        string Key { get; }
        DateTime CreatedDate { get; }
        int Version { get; }
        object Model { get; }
        ContentMetadataProvider Provider { get; }
    }

    public class Content<TContentModel> : IContent
        where TContentModel : class, new()
    {
        #region IContent members

        public string Key { get; }
        public DateTime CreatedDate { get; }
        public int Version { get; }
        object IContent.Model => Model;
        public ContentMetadataProvider Provider { get; }

        #endregion

        public TContentModel Model { get; }

        public Content(string key, TContentModel contentModel, ContentMetadataProvider contentProvider)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Version = 1;
            CreatedDate = DateTime.UtcNow;
            Model = contentModel ?? throw new ArgumentNullException(nameof(contentModel));
            Provider = contentProvider ?? throw new ArgumentNullException(nameof(contentProvider));
        }
        public Content(IContentDocument document, TContentModel contentModel, ContentMetadataProvider contentProvider)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            Key = document.Key;
            CreatedDate = document.CreatedDate;
            Version = document.Version;
            Model = contentModel ?? throw new ArgumentNullException(nameof(contentModel));
            Provider = contentProvider ?? throw new ArgumentNullException(nameof(contentProvider));

            if (contentProvider.ModelType != contentModel.GetType())
                throw new ArgumentException();
        }
    }
}