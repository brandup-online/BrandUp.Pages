using BrandUp.Pages.Content;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Metadata
{
    public class PageMetadataProvider
    {
        private readonly List<PageMetadataProvider> derivedTypes = new List<PageMetadataProvider>();
        private readonly Content.Fields.TextField titleField;

        public ContentMetadataProvider ContentMetadata { get; }
        public string Name => ContentMetadata.Name;
        public string Title => ContentMetadata.Title;
        public string Description => ContentMetadata.Description;
        public Type ContentType => ContentMetadata.ModelType;
        public PageMetadataProvider ParentMetadata { get; }
        public IEnumerable<PageMetadataProvider> DerivedTypes => derivedTypes;

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

                if (!(field is Content.Fields.TextField title))
                    throw new InvalidOperationException();

                titleField = title;
            }

            if (titleField == null)
                throw new InvalidOperationException();
        }

        public object CreatePageModel(string title = null)
        {
            var model = ContentMetadata.CreateModelInstance();

            if (title == null)
                title = GetPageName(model);

            if (string.IsNullOrEmpty(title))
            {
                title = "New page";
                titleField.SetModelValue(model, title);
            }

            return model;
        }
        public string GetPageName(object pageModel)
        {
            if (pageModel == null)
                throw new ArgumentNullException(nameof(pageModel));

            return (string)titleField.GetModelValue(pageModel);
        }
        public bool IsInheritedOf(PageMetadataProvider baseMetadataProvider)
        {
            if (baseMetadataProvider == null)
                throw new ArgumentNullException(nameof(baseMetadataProvider));

            return ContentType.IsSubclassOf(baseMetadataProvider.ContentType);
        }

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