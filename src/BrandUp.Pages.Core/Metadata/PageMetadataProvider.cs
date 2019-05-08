using BrandUp.Pages.Content;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Metadata
{
    public class PageMetadataProvider
    {
        private readonly List<PageMetadataProvider> derivedTypes = new List<PageMetadataProvider>();
        private readonly Content.Fields.ITextField titleField;

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

            PageTitleAttribute titleAttribute = null;
            foreach (var field in contentMetadata.Fields)
            {
                titleAttribute = field.Member.Member.GetCustomAttribute<PageTitleAttribute>(true);
                if (titleAttribute == null)
                    continue;

                if (!(field is Content.Fields.ITextField title))
                    throw new InvalidOperationException();

                titleField = title;
            }

            if (titleField == null)
                throw new InvalidOperationException();
        }

        #region Methods

        public object CreatePageModel(string title = null)
        {
            var model = ContentMetadata.CreateModelInstance();

            if (string.IsNullOrEmpty(title))
            {
                title = GetPageTitle(model);

                if (string.IsNullOrEmpty(title))
                    title = "New page";

                SetPageTitle(model, title);
            }
            else
                SetPageTitle(model, title);

            return model;
        }
        public string GetPageTitle(object pageModel)
        {
            if (pageModel == null)
                throw new ArgumentNullException(nameof(pageModel));

            return (string)titleField.GetModelValue(pageModel);
        }
        public void SetPageTitle(object pageModel, string title)
        {
            if (pageModel == null)
                throw new ArgumentNullException(nameof(pageModel));
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException();

            titleField.SetModelValue(pageModel, title);
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