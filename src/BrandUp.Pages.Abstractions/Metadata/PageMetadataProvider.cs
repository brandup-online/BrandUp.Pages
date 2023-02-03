using BrandUp.Pages.Content;

namespace BrandUp.Pages.Metadata
{
    public class PageMetadataProvider : IEquatable<PageMetadataProvider>
    {
        private readonly List<PageMetadataProvider> derivedTypes = new();

        #region Properties

        public ContentMetadataProvider ContentMetadata { get; }
        public string Name => ContentMetadata.Name;
        public string Title => ContentMetadata.Title;
        public string Description => ContentMetadata.Description;
        public Type ContentType => ContentMetadata.ModelType;
        public PageMetadataProvider ParentMetadata { get; }
        public IEnumerable<PageMetadataProvider> DerivedTypes => derivedTypes;
        public bool AllowCreateModel => !ContentMetadata.IsAbstract;

        #endregion

        internal PageMetadataProvider(ContentMetadataProvider contentMetadata, PageMetadataProvider parentPageMetadata)
        {
            ContentMetadata = contentMetadata;
            ParentMetadata = parentPageMetadata;

            parentPageMetadata?.derivedTypes.Add(this);
        }

        #region Methods

        public object CreatePageModel()
        {
            return ContentMetadata.CreateModelInstance();
        }
        public string GetPageHeader(object pageModel)
        {
            if (pageModel == null)
                throw new ArgumentNullException(nameof(pageModel));

            return ContentMetadata.GetContentTitle(pageModel);
        }
        public void SetPageHeader(object pageModel, string value)
        {
            if (pageModel == null)
                throw new ArgumentNullException(nameof(pageModel));

            ContentMetadata.SetContentTitle(pageModel, value);
        }
        public bool IsInherited(PageMetadataProvider baseMetadataProvider)
        {
            if (baseMetadataProvider == null)
                throw new ArgumentNullException(nameof(baseMetadataProvider));

            return ContentMetadata.IsInherited(baseMetadataProvider.ContentMetadata);
        }
        public bool IsInheritedOrEqual(PageMetadataProvider baseMetadataProvider)
        {
            if (baseMetadataProvider == null)
                throw new ArgumentNullException(nameof(baseMetadataProvider));

            return ContentMetadata.IsInheritedOrEqual(baseMetadataProvider.ContentMetadata);
        }

        #endregion

        #region IEquatable members

        public bool Equals(PageMetadataProvider other)
        {
            if (other is null or not PageMetadataProvider)
                return false;

            return ContentType == other.ContentType;
        }

        #endregion

        #region Object members

        public override string ToString()
        {
            return Name;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as PageMetadataProvider);
        }
        public override int GetHashCode()
        {
            return ContentType.GetHashCode();
        }

        #endregion
    }
}