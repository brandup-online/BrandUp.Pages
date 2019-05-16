using BrandUp.Pages.Content;
using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Metadata
{
    public class PageMetadataProvider
    {
        private readonly List<PageMetadataProvider> derivedTypes = new List<PageMetadataProvider>();

        #region Properties

        public ContentMetadataProvider ContentMetadata { get; }
        public string Name => ContentMetadata.Name;
        public string Title => ContentMetadata.Title;
        public string Description => ContentMetadata.Description;
        public Type ContentType => ContentMetadata.ModelType;
        public PageMetadataProvider ParentMetadata { get; }
        public IEnumerable<PageMetadataProvider> DerivedTypes => derivedTypes;

        #endregion

        internal PageMetadataProvider(ContentMetadataProvider contentMetadata, PageMetadataProvider parentPageMetadata)
        {
            ContentMetadata = contentMetadata;
            ParentMetadata = parentPageMetadata;

            if (parentPageMetadata != null)
                parentPageMetadata.derivedTypes.Add(this);
        }

        #region Methods

        public object CreatePageModel(string title = null)
        {
            var model = ContentMetadata.CreateModelInstance();

            if (string.IsNullOrEmpty(title))
            {
                title = ContentMetadata.GetContentTitle(model);

                if (string.IsNullOrEmpty(title))
                    title = "New page";

                ContentMetadata.SetContentTitle(model, title);
            }
            else
                ContentMetadata.SetContentTitle(model, title);

            return model;
        }
        public string GetPageTitle(object pageModel)
        {
            if (pageModel == null)
                throw new ArgumentNullException(nameof(pageModel));

            return ContentMetadata.GetContentTitle(pageModel);
        }
        public bool IsInherited(PageMetadataProvider baseMetadataProvider)
        {
            if (baseMetadataProvider == null)
                throw new ArgumentNullException(nameof(baseMetadataProvider));

            return ContentMetadata.IsInherited(baseMetadataProvider.ContentMetadata);
        }

        #endregion

        #region Object members

        public override string ToString()
        {
            return "Name";
        }
        public override int GetHashCode()
        {
            return ContentType.GetHashCode();
        }

        #endregion
    }
}